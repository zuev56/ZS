using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Common.Extensions;
using Zs.Home.Application.Features.VkUsers;
using Zs.Home.Jobs.Hangfire.Extensions;
using Zs.Home.Jobs.Hangfire.Hangfire;
using Zs.Home.Jobs.Hangfire.Notification;

namespace Zs.Home.Jobs.Hangfire.UserWatcher;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class UserWatcherJob : IJob
{
    private static readonly TimeSpan _alarmInterval = 3.Hours();
    private static DateTime? _lastAlarmUtcDate = DateTime.UtcNow - _alarmInterval;
    private static readonly Dictionary<int, bool> _userIdToIsInactive = [];

    private readonly IUserWatcher _userWatcher;
    private readonly UserWatcherSettings _settings;
    private readonly Notifier _notifier;
    private readonly ILogger<UserWatcherJob> _logger;

    public UserWatcherJob(
        IUserWatcher userWatcher,
        IOptions<UserWatcherSettings> settings,
        Notifier notifier,
        ILogger<UserWatcherJob> logger)
    {
        _userWatcher = userWatcher;
        _notifier = notifier;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogJobStart();

        var inactiveUsersInfo = await DetectInactiveUsersAsync(ct);

        await _notifier.SendNotificationAsync(inactiveUsersInfo, ct);

        _logger.LogJobFinish(sw.Elapsed);
    }

    private async Task<string> DetectInactiveUsersAsync(CancellationToken ct)
    {
        var result = new StringBuilder();
        var usersWithInactiveTime = await _userWatcher.GetUsersWithInactiveTimeAsync(_settings.TrackedIds, ct);

        foreach (var (user, inactiveTime) in usersWithInactiveTime)
        {
            if (inactiveTime < _settings.InactiveHoursLimit.Hours())
            {
                if (_userIdToIsInactive.TryGetValue(user.Id, out _))
                    result.AppendLine($"{user.GetFullName()} is back online!");

                _userIdToIsInactive[user.Id] = false;
                continue;
            }

            if (DateTime.UtcNow < _lastAlarmUtcDate + _alarmInterval)
                continue;

            _lastAlarmUtcDate = DateTime.UtcNow;
            _userIdToIsInactive[user.Id] = true;
            var inactiveTimeString = inactiveTime.TotalDays >= 1
                ? $@"{(int)inactiveTime.TotalDays} {(inactiveTime.TotalDays > 2 ? "days" : "day")} {inactiveTime:hh\:mm}"
                : $@"{inactiveTime:hh\:mm\:ss}";
            result.AppendLine($"{user.GetFullName()} is not active for {inactiveTimeString}");
        }

        return result.ToString().Trim();
    }
}

static class UserExtensions
{
    internal static string GetFullName(this User user) => $"{user.FirstName} {user.LastName}";
}
