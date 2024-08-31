using ChatAdmin.Bot.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Data.Enums;
using Zs.Bot.Services.Commands;
using Zs.Bot.Services.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Extensions;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Scheduler;

namespace ChatAdmin.Bot;

internal sealed class ChatAdmin : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatAdmin> _logger;
    private readonly IMessenger _messenger;
    private readonly IScheduler _scheduler;
    private readonly IMessageProcessor _messageProcessor;
    private readonly IDbClient _dbClient;
    private readonly ISeqService _seqService;
    private readonly IConnectionAnalyser _connectionAnalyser;
    private string _botName;

    public ChatAdmin(
        IConfiguration configuration,
        IConnectionAnalyser connectionAnalyser,
        IMessenger messenger,
        IMessageProcessor messageProcessor,
        IScheduler scheduler,
        IDbClient dbClient,
        ISeqService seqService,
        ILogger<ChatAdmin> logger)
    {
        try
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _connectionAnalyser = connectionAnalyser ?? throw new ArgumentNullException(nameof(connectionAnalyser));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _messageProcessor = messageProcessor ?? throw new ArgumentNullException(nameof(messageProcessor));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _dbClient = dbClient ?? throw new ArgumentNullException(nameof(dbClient));
            _seqService = seqService ?? throw new ArgumentNullException(nameof(seqService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            CreateJobs();
        }
        catch (Exception ex)
        {
            var tiex = new TypeInitializationException(typeof(ChatAdmin).FullName, ex);
            _logger.LogErrorIfNeed(tiex, $"{nameof(ChatAdmin)} initialization error");
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _messenger.MessageReceived += Messenger_MessageReceived;
        _connectionAnalyser.ConnectionStatusChanged += СonnectionAnalyser_StatusChanged;
        _messageProcessor.ChatStateService.LimitsDefined += MessageProcessor_LimitsDefined;

        var botInfo = await _messenger.GetBotInfoAsync(cancellationToken).ConfigureAwait(false);
        _botName = botInfo.EnumerateObject().FirstOrDefault(i => i.Name == "Username").Value.ToString();

        _connectionAnalyser.Start(5000, 30000);
        _scheduler.Start(3000, 1000);

        string startMessage = $"Bot '{nameof(ChatAdmin)}' started."
            + Environment.NewLine + Environment.NewLine
            + RuntimeInformationWrapper.GetRuntimeInfo();

        await _messenger.AddMessageToOutboxAsync(startMessage, Role.Owner, Role.Admin).ConfigureAwait(false);
        _logger.LogInformationIfNeed(startMessage);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _messenger.MessageReceived -= Messenger_MessageReceived;
        _connectionAnalyser.ConnectionStatusChanged -= СonnectionAnalyser_StatusChanged;
        _messageProcessor.ChatStateService.LimitsDefined -= MessageProcessor_LimitsDefined;

        _connectionAnalyser.Stop();
        _scheduler.Stop();
        _logger.LogInformationIfNeed($"{nameof(ChatAdmin)} stopped");

        return Task.CompletedTask;
    }

    private async void MessageProcessor_LimitsDefined(string messageText)
    {
        if (DateTime.Now.Hour >= _configuration.GetValue<int>("Notifier:Time:FromHour"))
            await _messenger.AddMessageToOutboxAsync(messageText, Role.Owner, Role.Admin).ConfigureAwait(false);
    }

    private void СonnectionAnalyser_StatusChanged(ConnectionStatus status)
    {
        if (status == ConnectionStatus.Ok)
            _messageProcessor?.ChatStateService.SetInternetRepairDate(DateTime.UtcNow);
        else
        {
            _messageProcessor?.ChatStateService.SetInternetRepairDate(null);
            _messageProcessor?.RemoveBanWarnings();
        }
    }

    private void Messenger_MessageReceived(object sender, MessageActionEventArgs e)
    {
        _messageProcessor.ProcessGroupMessage(e.Message);

        if (BotCommand.IsCommand(e.Message.Text, _botName))
        {
            var command = BotCommand.GetCommandFromMessage(e.Message);
            bool commandsAreAllowed = _configuration.GetValue<bool>("ChatAdmin:Chat:AllowCommands");
            bool userIsAdminOrOwner = e.User.UserRole >= Role.Admin;
            bool commandIsFromGroup = e.Chat.Id == _configuration.GetValue<int>("ChatAdmin:Chat:Id");

            if ((!commandsAreAllowed && !userIsAdminOrOwner && commandIsFromGroup)
                || (commandsAreAllowed && commandIsFromGroup && command.TargetBotName != _botName))
            {
                e.IsHandled = true;
            }
        }
    }

    private async void Job_ExecutionCompleted(IJob<string> job, IOperationResult<string> result)
    {
        if (result?.IsSuccess == false)
            _logger.LogWarningIfNeed("Job \"{Job}\" execution failed. Result: {Result}", job.Description, result.Value);

        if (result?.IsSuccess == true && result.Value != null
            && DateTime.Now.Hour >= _configuration.GetValue<int>("Notifier:Time:FromHour")
            && DateTime.Now.Hour < _configuration.GetValue<int>("Notifier:Time:ToHour"))
        {
            await _messenger.AddMessageToOutboxAsync(result.Value, Role.Owner, Role.Admin).ConfigureAwait(false);
        }
    }

    /// <summary>Creating a <see cref="Job"/> list for <see cref="Scheduler"/> instance</summary>
    private void CreateJobs()
    {
        TimeSpan utcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);

        CreateChatAdminJobs(utcOffset);

        var logProcessStateInfo = new ProgramJob(
            method: () => Task.Run(() => _logger.LogProcessState(Process.GetCurrentProcess())),
            period: TimeSpan.FromDays(1),
            startUtcDate: DateTime.UtcNow + TimeSpan.FromMinutes(1),
            description: "logProcessStateInfo"
        );
        _scheduler.Jobs.Add(logProcessStateInfo);
    }

    private void CreateChatAdminJobs(TimeSpan utcOffset)
    {
        var sendYesterdaysStatistics = new SqlJob(
            sqlQuery: $"select ca.sf_job_get_yesterdays_statistics({_configuration.GetValue<int>("ChatAdmin:Chat:Id")})",
            resultType: QueryResultType.String,
            dbClient: _dbClient,
            period: TimeSpan.FromDays(1),
            startUtcDate: DateTime.UtcNow.Date + TimeSpan.FromHours(24 + 10) - utcOffset,
            description: "sendYesterdaysStatistics"
        );
        sendYesterdaysStatistics.ExecutionCompleted += Job_ExecutionCompleted;
        _scheduler.Jobs.Add(sendYesterdaysStatistics);

        var resetLimits = new ProgramJob(
            method: _messageProcessor.ChatStateService.SetLimitsFromConfiguration,
            period: TimeSpan.FromDays(1),
            startUtcDate: DateTime.UtcNow.Date + TimeSpan.FromDays(1) - utcOffset,
            description: "resetLimits"
        );
        _scheduler.Jobs.Add(resetLimits);
    }
}
