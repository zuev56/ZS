using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zs.Bot.Data.Abstractions;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;

namespace ChatAdmin.Bot;

internal sealed class ChatStateService
{
    private readonly IConfiguration _configuration;
    private readonly IMessagesRepository _messagesRepo;
    private readonly ILogger<ChatStateService> _logger;

    /// <summary>Chat identifier, where the bot process messages</summary>
    public int DefaultChatId { get; }
    /// <summary>Identifiers of users without message limit</summary>
    public int[] UnaccountableUserIds { get; }
    /// <summary>Personal message limit before warning</summary>
    public int LimitHi { get; set; }
    /// <summary>Personal message limit before ban</summary>
    public int LimitHiHi { get; set; }
    /// <summary>Messages count allowed for unbanned user until the and of a day</summary>
    public int LimitAfterBan { get; set; }
    /// <summary>Daily chat messages limit before starting of personal accounting</summary>
    public int AccountingStartsAfter { get; set; }
    /// <summary>The time interval to wait after repairing the internet connection</summary>
    public TimeSpan WaitAfterConnectionRepaired { get; }
    /// <summary>Indicates that limits are defined</summary>
    public bool LimitsAreDefined { get; set; }
    /// <summary>The date when started personal message counting</summary>
    public DateTime? AccountingStartDate { get; set; }
    /// <summary>The date when the internet connection was repaired</summary>
    public DateTime? ConnectionRepairDate { get; set; }
    /// <summary>Occurs when limits are redefined</summary>

    public event Action<string> LimitsDefined;

    public ChatStateService(
        IConfiguration configuration,
        IMessagesRepository messagesRepo,
        ILogger<ChatStateService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _messagesRepo = messagesRepo ?? throw new ArgumentNullException(nameof(messagesRepo));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var defaultChatIdSection = "ChatAdmin:Chat:Id";
        DefaultChatId = _configuration[defaultChatIdSection] != null
            ? _configuration.GetValue<int>(defaultChatIdSection)
            : throw new ConfigurationSectionNotFoundException(defaultChatIdSection);

        UnaccountableUserIds = _configuration.GetSection("ChatAdmin:Chat:UnaccountableUserIds").Get<int[]>() ?? Array.Empty<int>();
        WaitAfterConnectionRepaired = TimeSpan.FromSeconds(_configuration.GetValue<int>("ChatAdmin:WaitAfterConnectionRepairedSec"));

        SetLimitsFromConfiguration();
    }

    internal void ResetLimits()
    {
        LimitHi = -1;
        LimitHiHi = -1;
        LimitAfterBan = -1;
        AccountingStartsAfter = -1;
        LimitsAreDefined = false;
        AccountingStartDate = null;
    }

    /// <summary> Reset parameters to theirs configuration values </summary>
    internal Task SetLimitsFromConfiguration()
    {
        try
        {
            LimitHi = _configuration.GetValue<int>("ChatAdmin:MessageLimitHi");
            LimitHiHi = _configuration.GetValue<int>("ChatAdmin:MessageLimitHiHi");
            LimitAfterBan = _configuration.GetValue<int>("ChatAdmin:MessageLimitAfterBan");
            AccountingStartsAfter = _configuration.GetValue<int>("ChatAdmin:AccountingStartsAfter");
            AccountingStartDate = null;

            _logger.LogInformationIfNeed("Limits set from configuration file", new
            {
                MessageLimitHi = LimitHi,
                MessageLimitHiHi = LimitHiHi,
                MessageLimitAfterBan = LimitAfterBan,
                AccountingStartsAfter
            });

            Volatile.Read(ref LimitsDefined)?.Invoke(GetLimitInfo());
        }
        catch (Exception ex)
        {
            ResetLimits();

            _logger.LogErrorIfNeed(ex, "Reset limits error");
        }
        return Task.CompletedTask;
    }

    /// <summary> Set date when connection to the internet was repaired </summary>
    internal async Task SetInternetRepairDate(DateTime? date)
    {
        ConnectionRepairDate = date;

        if (ConnectionRepairDate is null)
        {
            LimitsAreDefined = false;
            AccountingStartDate = null;
            _logger.LogWarningIfNeed("The internet connection has been lost");
        }
        else
        {
            await Task.Run(async () =>
            {
                await Task.Delay(WaitAfterConnectionRepaired).ConfigureAwait(false);
                await SetLimitsAccordingToCurrentChatState().ConfigureAwait(false);

                Volatile.Read(ref LimitsDefined)?.Invoke(GetLimitInfo());
            }).ConfigureAwait(false);
        }
    }

    internal async Task SetLimitsAccordingToCurrentChatState()
    {
        int maxAcountedMessagesFromUser = -1;
        int dailyMsgCount = -1;
        try
        {
            // 1. Accounting has not started yet
            //   1.1. After service start => return
            //   1.2. After disconnecting from the internet => override limits
            // 2. Accounting has already started => override limits

            // Telegram message date is GMT. But now everything depends on the InsertDate.

            var userMessageCounts = new Dictionary<int, int>();
            if (AccountingStartDate != null)
                userMessageCounts = await _messagesRepo.FindUserIdsAndMessagesCountSinceDate(DefaultChatId, AccountingStartDate).ConfigureAwait(false);

            maxAcountedMessagesFromUser = userMessageCounts.Count > 0
                ? userMessageCounts.Max(i => i.Value)
                : 0;

            dailyMsgCount = (await _messagesRepo.FindDailyMessages(DefaultChatId).ConfigureAwait(false)).Count(m => !m.IsDeleted);

            AccountingStartsAfter = dailyMsgCount > AccountingStartsAfter
                ? dailyMsgCount + 2
                : AccountingStartsAfter;

            if (AccountingStartsAfter > dailyMsgCount)
                AccountingStartDate = null;

            // if the accounting is not started, keep old limits
            var configAccountingStartsAfter = _configuration.GetValue<int>("ChatAdmin:AccountingStartsAfter");

            if (AccountingStartDate == null
                && AccountingStartsAfter == configAccountingStartsAfter)
            {
                LimitsAreDefined = true;
                return;
            }

            if (LimitHi < maxAcountedMessagesFromUser || LimitHiHi < maxAcountedMessagesFromUser)
            {
                LimitHi = LimitHi > 0 ? maxAcountedMessagesFromUser + 2 : -1;
                LimitHiHi = LimitHi > 0 ? LimitHi + 5 : -1;
            }

            LimitsAreDefined = true;
        }
        catch (Exception ex)
        {
            ResetLimits();
            _logger.LogErrorIfNeed(ex, "Define actual limits error");
        }
        finally
        {
            _logger.LogInformationIfNeed("Limits defined", new
            {
                LimitHi,
                LimitHiHi,
                AccountingStartsAfter,
                AccountingStartDate,
                maxAcountedMessagesFromUser,
                dailyMsgCount
            });
        }
    }

    /// <summary> Get user-friendly information string about limits </summary>
    internal string GetLimitInfo()
    {
        var accountingStatus = AccountingStartDate is null
            ? "Accounting not started."
            : $"Accounting started at {AccountingStartDate}.";

        return $"Warning after {LimitHi} messages.\n"
             + $"Ban for three hours after {LimitHiHi} messages.\n"
             + $"Accounting starts after {AccountingStartsAfter} messages per day.\n"
             + accountingStatus;
    }
}
