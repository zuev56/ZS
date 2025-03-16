using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Zs.Bot.Data.Repositories;
using Zs.Bot.Services.Messaging;
using Zs.Common.Extensions;
using Zs.Common.Models;

namespace Zs.Home.Bot.Interaction;

internal sealed class Notifier
{
    private readonly IBotClient _botClient;
    private readonly IChatsRepository _chatsRepository;
    private readonly IMessagesRepository _messagesRepository;
    private readonly NotifierSettings _notifierSettings;
    private readonly BotSettings _botSettings;

    public Notifier(
        IBotClient botClient,
        IChatsRepository chatsRepository,
        IMessagesRepository messagesRepository,
        IOptions<NotifierSettings> notifierOptions,
        IOptions<BotSettings> botOptions)
    {
        _botClient = botClient;
        _chatsRepository = chatsRepository;
        _messagesRepository = messagesRepository;
        _botSettings = botOptions.Value;
        _notifierSettings = notifierOptions.Value;
    }

    public Task ForceNotifyAsync(string notification)
    {
        var preparedMessage = GetPreparedMessage(notification);

        foreach (var rawUserId in _botSettings.PrivilegedUserRawIds)
        {
            _chatsRepository.FindByRawIdAsync(rawUserId)
                .ContinueWith(task => _botClient.SendMessageAsync(preparedMessage, task.Result!));
        }

        return Task.CompletedTask;
    }

    private static string GetPreparedMessage(string message)
        => message.ReplaceEndingWithThreeDots(4000);

    public async Task NotifyAsync(string message)
    {
        var curHour = DateTime.Now.Hour;
        if (string.IsNullOrWhiteSpace(message) || curHour < _notifierSettings.FromHour || curHour >= _notifierSettings.ToHour)
            return;

        var preparedMessage = GetPreparedMessage(message);
        await ForceNotifyAsync(preparedMessage);
    }

    public async Task NotifyOnceADayAsync(string message, string templateForExclusion)
    {
        var preparedMessage = GetPreparedMessage(message);
        var utcToday = DateTime.Today.ToUniversalTime();
        var dateRange = new DateTimeRange(utcToday, DateTime.MaxValue);
        // TODO: в FindWithTextAsync происходит неправильная конвертация времени - разница 6 часов. Надо поправить в Zs.Bot.Data
        var todayAlerts = await _messagesRepository.FindWithTextAsync(_botSettings.OwnerChatRawId, templateForExclusion, dateRange);
        if (todayAlerts.All(m => BotSettings.GetMessageText(m)?.WithoutDigits() != message.WithoutDigits()))
            await NotifyAsync(preparedMessage);
    }
}
