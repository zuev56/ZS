using Microsoft.Extensions.Options;
using Zs.Bot.Data.Storages;
using Zs.Bot.Services.Commands;
using Zs.Bot.Services.Messaging;
using Zs.Bot.Services.Pipeline;
using Zs.Bot.Telegram.Extensions;
using static System.Environment;

namespace Zs.DemoBot;

internal sealed class DemoBot : IHostedService
{
    private readonly IBotClient _botClient;
    private readonly ILogger<DemoBot> _logger;
    private readonly Settings _settings;

    public DemoBot(
        IBotClient botClient,
        IMessageDataStorage messageDataSaver,
        ICommandManager commandManager,
        IOptions<Settings> settings,
        ILogger<DemoBot> logger)
    {
        _botClient = botClient;
        _logger = logger;
        _settings = settings.Value;

        SetupMessagePipeline(messageDataSaver, commandManager);
    }

    private void SetupMessagePipeline(IMessageDataStorage messageDataSaver, ICommandManager commandManager)
    {
        var authorization = new AuthorizationStep(_botClient, _settings.PrivilegedUserRawIds);

        _botClient
            .UseLogger(_logger)
            .UseMessageDataSaver(messageDataSaver)
            .Use(authorization)
            .UseCommandManager(commandManager, message => message.GetText(), _settings.BotName);
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var chat = _settings.OwnerChatRawId.ToChat();
        var startMessageText = $"Bot started.{NewLine}"
                             + $"Command examples:{NewLine}"
                             + $"/sql select count(*) from messages{NewLine}"
                             + $"/cli pwd";

        await _botClient.SendMessageAsync(startMessageText, chat, cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}