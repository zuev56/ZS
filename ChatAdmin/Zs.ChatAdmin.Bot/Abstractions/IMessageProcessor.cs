using System;
using System.Threading.Tasks;
using Zs.Bot.Data.Models;

namespace ChatAdmin.Bot.Abstractions;

internal interface IMessageProcessor
{
    public ChatStateService ChatStateService { get; }
    Task ProcessGroupMessage(Message incomingMessage);
    Task DeleteBotCommandsAndAnswersInTimeRange(int chatId, int botUserId, string botName, DateTime fromDate, DateTime toDate);
    Task RemoveBanWarnings();
}
