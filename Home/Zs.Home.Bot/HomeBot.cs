using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Bot.Data.Storages;
using Zs.Bot.Services.Commands;
using Zs.Bot.Services.Messaging;
using Zs.Bot.Services.Pipeline;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Scheduling;
using Zs.Common.Utilities;
using Zs.Home.Application.Features.Hardware;
using Zs.Home.Application.Features.Seq;
using Zs.Home.Bot.Interaction;
using Zs.Home.Bot.Interaction.MessagePipeline;
using static Zs.Home.Application.Features.VkUsers.Constants;

namespace Zs.Home.Bot;

internal sealed class HomeBot : IHostedService
{
    private readonly IBotClient _botClient;
    private readonly IMessageDataStorage _messageDataStorage;
    private readonly IScheduler _scheduler;
    private readonly Notifier _notifier;
    private readonly BotSettings _botSettings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HomeBot> _logger;

    public HomeBot(
        IBotClient botClient,
        IMessageDataStorage messageDataStorage,
        IScheduler scheduler,
        Notifier notifier,
        IOptions<BotSettings> botOptions,
        IServiceProvider serviceProvider,
        ILogger<HomeBot> logger)
    {
        _botClient = botClient;
        _messageDataStorage = messageDataStorage;
        _scheduler = scheduler;
        _notifier = notifier;
        _botSettings = botOptions.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;

        SetupMessagePipeline();
        CreateJobs();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _scheduler.Start(3.Seconds(), 1.Seconds());

            var startMessage = $"{nameof(HomeBot)} started."
                               + Environment.NewLine + Environment.NewLine
                               + RuntimeInformationWrapper.GetRuntimeInfo();
            await _notifier.ForceNotifyAsync(startMessage);

            _logger.LogInformation(startMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bot starting error");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _scheduler.Stop();
        _logger.LogInformation("Bot stopped");

        return Task.CompletedTask;
    }

    private void SetupMessagePipeline()
    {
        // TODO: Вынести из этого класса
        var authorization = new Authorization(_botClient, _botSettings.PrivilegedUserRawIds);
        var localMessageHandler = new MessageHandler(_serviceProvider.GetRequiredService<CommandHandler>());
        var commandManager = _serviceProvider.GetRequiredService<ICommandManager>();

        _botClient
            .UseLogger(_logger)
            .UseMessageDataSaver(_messageDataStorage)
            .Use(authorization)
            .Use(localMessageHandler)
            .UseCommandManager(commandManager, BotSettings.GetMessageText, _botSettings.Name);
    }

    private void CreateJobs()
    {
        // Наверное, тут стоит оставить джоб, который будет проверять работоспособность Hangfire (убеждаться, что там выполняются задачи)

        // TODO: Вынести из этого проекта
        var hardwareMonitor = _serviceProvider.GetRequiredService<IHardwareMonitor>();
        var seqEventsInformer = _serviceProvider.GetRequiredService<ISeqEventsInformer>();

        _scheduler.Jobs.Add(hardwareMonitor.Job);
        _scheduler.Jobs.Add(seqEventsInformer.DayEventsInformerJob);
        _scheduler.Jobs.Add(seqEventsInformer.NightEventsInformerJob);
        _scheduler.Jobs.Add(LogProcessStateJob());

        _scheduler.SetDefaultExecutionCompletedHandler<string>(Job_ExecutionCompleted);
    }

    private ProgramJob LogProcessStateJob()
    {
        var logProcessStateInfo = new ProgramJob(
            period: 1.Days(),
            method: () => Task.Run(() => _logger.LogProcessState(Process.GetCurrentProcess())),
            startUtcDate: DateTime.UtcNow + 1.Minutes(),
            description: "logProcessStateInfo"
        );
        return logProcessStateInfo;
    }

    // ReSharper disable once AsyncVoidMethod
    private async void Job_ExecutionCompleted(Job<string> job, Result<string> result)
    {
        try
        {
            if (result.Successful == false)
            {
                _logger.LogWarning("Job \"{Job}\" execution failed {FaultCode}, {FaultMessage}", job.Description, result.Fault!.Code, result.Fault!.Message);
                return;
            }

            if (job.Description == InactiveUsersInformer)
            {
                await _notifier.NotifyOnceADayAsync(result.Value, "is not active for");
            }
            else
            {
                await _notifier.NotifyAsync(result.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job's ExecutionCompleted handler error");
        }
    }
}
