using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Bot.Data.Repositories;
using Zs.Bot.Services.Messaging;
using Zs.Common.Extensions;
using Zs.Common.Models;

namespace Zs.Home.Bot.Interaction;

internal sealed class Notifier
{
    private readonly IBotClient _botClient;
    //private readonly IChatsRepository _chatsRepository;
    //private readonly IMessagesRepository _messagesRepository;
    private readonly NotifierSettings _notifierSettings;
    private readonly BotSettings _botSettings;
    private readonly ILogger<Notifier> _logger;

    public Notifier(
        IBotClient botClient,
        //IChatsRepository chatsRepository,
        //IMessagesRepository messagesRepository,
        IOptions<NotifierSettings> notifierOptions,
        IOptions<BotSettings> botOptions,
        ILogger<Notifier> logger)
    {
        _botClient = botClient;
        //_chatsRepository = chatsRepository;
        //_messagesRepository = messagesRepository;
        _botSettings = botOptions.Value;
        _notifierSettings = notifierOptions.Value;
        _logger = logger;
    }

    public Task ForceNotifyAsync(string notification)
    {
        var preparedMessage = GetPreparedMessage(notification);

        foreach (var rawUserId in _botSettings.PrivilegedUserRawIds)
        {
            _botClient.SendMessageAsync(preparedMessage, rawUserId);
            // await _chatsRepository.FindByRawIdAsync(rawUserId)
            //     .ContinueWith(task => _botClient.SendMessageAsync(preparedMessage, task.Result!));
        }
        return Task.CompletedTask;
    }

    private static string GetPreparedMessage(string message)
        => message.ReplaceEndingWithThreeDots(4000);

    public async Task NotifyAsync(string message)
    {
        var curHour = DateTime.Now.Hour;
        if (string.IsNullOrWhiteSpace(message) || curHour < _notifierSettings.FromHour || curHour >= _notifierSettings.ToHour)
        {
            _logger.LogInformationIfNeed($"Quiet hours before: {_notifierSettings.FromHour} and after: {_notifierSettings.ToHour}, now: {curHour}. Message not sent: {message}");
            return;
        }

        var preparedMessage = GetPreparedMessage(message);
        await ForceNotifyAsync(preparedMessage);
    }
}
