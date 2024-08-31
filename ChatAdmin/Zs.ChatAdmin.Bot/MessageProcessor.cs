using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChatAdmin.Bot.Abstractions;
using ChatAdmin.Bot.Models;
using Microsoft.Extensions.Logging;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Enums;
using Zs.Bot.Data.Models;
using Zs.Bot.Services.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Extensions;

namespace ChatAdmin.Bot;

internal sealed class MessageProcessor : IMessageProcessor
{
    private readonly IMessenger _messenger;
    private readonly IChatsRepository _chatsRepo;
    private readonly IUsersRepository _usersRepo;
    private readonly IMessagesRepository _messagesRepo;
    private readonly IBansRepository _bansRepo;
    private readonly IDbClient _dbClient;
    private readonly ILogger<MessageProcessor> _logger;
    public ChatStateService ChatStateService { get; }

    public MessageProcessor(
        ChatStateService chatStateService,
        IMessenger messenger,
        IChatsRepository chatsRepo,
        IUsersRepository usersRepo,
        IMessagesRepository messagesRepo,
        IBansRepository bansRepo,
        IDbClient dbClient,
        ILogger<MessageProcessor> logger)
    {

        try
        {
            ChatStateService = chatStateService ?? throw new ArgumentNullException(nameof(chatStateService));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _chatsRepo = chatsRepo ?? throw new ArgumentNullException(nameof(chatsRepo));
            _usersRepo = usersRepo ?? throw new ArgumentNullException(nameof(usersRepo));
            _messagesRepo = messagesRepo ?? throw new ArgumentNullException(nameof(messagesRepo));
            _bansRepo = bansRepo ?? throw new ArgumentNullException(nameof(bansRepo));
            _dbClient = dbClient ?? throw new ArgumentNullException(nameof(dbClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        catch (Exception ex)
        {
            throw new TypeInitializationException(typeof(MessageProcessor).FullName, ex);
        }
    }

    public async Task ProcessGroupMessage(Message incomingMessage)
    {
        // Начало индивидуального учёта после 100 сообщений в чате от любых пользователей с 00:00 текущего дня.
        // С начала учёта каждому доступно максимум 30 сообщений.
        // После 25-го сообщения с начала учёта выдать пользователю
        //     предупреждение о приближении к лимиту. При этом создаётся запись
        //     в таблице Ban и ставится пометка о том, что пользователь предупреждён.
        // При достижении лимита пользователь банится на 3 часа.
        //     Если лимит достигнут ближе к концу дня, бан продолжает своё действие
        //     до окончания трёхчасового периода. Если 3 часа бана прошло,
        //     а день не закончился, позволяем пользователю отправку 5-ти сообщений
        //     до начала следующего дня
        //
        // После восстановления интернета через 1 минуту происходит
        //     переопределение лимитов для того, чтобы не перетереть
        //     только что полученные сообщения
        //
        // P.S. этот алгоритм придумал заказчик

        if (incomingMessage is null)
            throw new ArgumentNullException(nameof(incomingMessage));

        if (!NeedMessageProcessing(incomingMessage))
            return;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            var processGroupMessageResult = await ExecuteSfProcessGroupMessage(incomingMessage).ConfigureAwait(false);

            await HandleSfProcessGroupMessageResult(processGroupMessageResult, incomingMessage).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogErrorIfNeed(ex, "Message processing error");
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogDebugIfNeed("Group message processed: {Elapsed}", stopwatch.Elapsed);
        }
    }

    private async Task<SfProcessGroupMessageResult> ExecuteSfProcessGroupMessage(Message incomingMessage)
    {
        var accountingStartDate = ChatStateService.AccountingStartDate is null
             ? "null"
             : $"'{ChatStateService.AccountingStartDate:yyyy-MM-ddTHH:mm:ss}'";

        var query = $@"select ca.sf_process_group_message(
                 _chat_id => {incomingMessage.ChatId},
                 _message_id => {incomingMessage.Id},
                 _accounting_start_date => {accountingStartDate},
                 _msg_limit_hi => {ChatStateService.LimitHi},
                 _msg_limit_hihi => { ChatStateService.LimitHiHi},
                 _msg_limit_after_ban => {ChatStateService.LimitAfterBan},
                 _start_account_after => {ChatStateService.AccountingStartsAfter})";

        var jsonResult = await _dbClient.GetQueryResultAsync(query).ConfigureAwait(false);
        _logger.LogTraceIfNeed("Incoming message sql-processing result: {Result}", new { JsonResult = jsonResult, MessageId = incomingMessage.Id });

        return JsonSerializer.Deserialize<SfProcessGroupMessageResult>(jsonResult);
    }

    private async Task HandleSfProcessGroupMessageResult(SfProcessGroupMessageResult sfResult, Message incomingMessage)
    {
        if (sfResult.Action.HasValue)
        {
            var chat = await _chatsRepo.FindByIdAsync(incomingMessage.ChatId).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(sfResult.MessageText)
                && sfResult.MessageText.Contains("<UserName>", StringComparison.InvariantCultureIgnoreCase))
            {
                var dbUser = await _usersRepo.FindByIdAsync(incomingMessage.UserId).ConfigureAwait(false);
                var userName = dbUser?.Name != null ? $"@{dbUser.Name}" : dbUser?.FullName ?? "UserName";
                sfResult.MessageText = sfResult.MessageText.Replace("<UserName>", userName, StringComparison.InvariantCultureIgnoreCase);
            }

            await PerformSfProcessGroupMessageResultAction(sfResult, chat, incomingMessage).ConfigureAwait(false);
        }
        else
        {
            await _messenger.AddMessageToOutboxAsync("Unknown message process result", Role.Owner, Role.Admin).ConfigureAwait(false);
        }
    }

    private async Task PerformSfProcessGroupMessageResultAction(SfProcessGroupMessageResult sfResult, Chat chat, Message message)
    {
        switch (sfResult.Action)
        {
            case SfResultAction.Continue: return;
            case SfResultAction.DeleteMessage:
                if (!await _messenger.DeleteMessageAsync(message).ConfigureAwait(false))
                {
                    await _messenger.AddMessageToOutboxAsync("Message deleting failed", Role.Owner, Role.Admin).ConfigureAwait(false);
                    _logger.LogWarningIfNeed("Message deleting failed", message);
                }
                return;
            case SfResultAction.SetAccountingStartDate:
                _messenger.AddMessageToOutbox(chat, sfResult.MessageText);
                ChatStateService.AccountingStartDate = sfResult.AccountingStartDate + TimeSpan.FromSeconds(1);
                return;
            case SfResultAction.SendMessageToGroup:
                _messenger.AddMessageToOutbox(chat, sfResult.MessageText, message);
                return;
            case SfResultAction.SendMessageToOwner:
                await _messenger.AddMessageToOutboxAsync(sfResult.MessageText, Role.Owner, Role.Admin).ConfigureAwait(false);
                return;
            default:
                await _messenger.AddMessageToOutboxAsync("Unknown action", Role.Owner, Role.Admin).ConfigureAwait(false);
                break;
        }
    }

    private bool NeedMessageProcessing(Message message)
    {
        if (message.ChatId != ChatStateService.DefaultChatId)
            return false;

        if (!ChatStateService.LimitsAreDefined)
        {
            _logger.LogInformationIfNeed("Limits are not defined. Group message won't be processed");
            return false;
        }

        if (ChatStateService.UnaccountableUserIds.Contains(message.UserId))
        {
            _logger.LogInformationIfNeed("The message from unaccountable user (Id: {DbUserId}) won't be processed", message.UserId);
            return false;
        }

        if ((DateTime.UtcNow - ChatStateService.ConnectionRepairDate) <= ChatStateService.WaitAfterConnectionRepaired)
        {
            _logger.LogInformationIfNeed("The message [{MessageId}] won't be processed. Internet was restored less then {WaitAfterConnectionRepaired} seconds ago", message.Id, ChatStateService.WaitAfterConnectionRepaired.TotalSeconds);
            return false;
        }

        return true;
    }

    public async Task DeleteBotCommandsAndAnswersInTimeRange(int chatId, int botUserId, string botName, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var rawFilteredMessages = await _messagesRepo.FindBotDialogMessagesInTimeRange(chatId, botUserId, botName, fromDate, toDate).ConfigureAwait(false);
            var botAnswers = rawFilteredMessages.Where(m => m.UserId == botUserId);
            var botCommands = rawFilteredMessages.Where(m => m.UserId != botUserId && Regex.IsMatch(m.Text.Trim(), $@"/[a-z0-9_]+\@{botName}"));

            foreach (var message in botAnswers.Union(botCommands))
                await _messenger.DeleteMessageAsync(message).ConfigureAwait(false);

            if (botCommands.Any() || botAnswers.Any())
                _logger.LogInformationIfNeed("Delete {CommandsCount} commands to {BotName} and {AnswersCount} answers from the bot", botCommands.Count(), botName, botAnswers.Count());
        }
        catch (Exception ex)
        {
            _logger.LogErrorIfNeed(ex, "Delete Bot commands and answers error");
        }
    }

    public async Task RemoveBanWarnings()
    {
        var warnings = await _bansRepo.FindAllTodaysBanWarningsAsync().ConfigureAwait(false);
        await _bansRepo.DeleteRangeAsync(warnings).ConfigureAwait(false);

        _logger.LogWarningIfNeed("Today's ban warnings removed");
    }
}
