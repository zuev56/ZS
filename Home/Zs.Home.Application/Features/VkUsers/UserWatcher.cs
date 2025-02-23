using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Common.Extensions;
using Zs.Common.Services.Http;
using Zs.Common.Services.Scheduling;
using static Zs.Home.Application.Features.VkUsers.Constants;

namespace Zs.Home.Application.Features.VkUsers;

internal sealed class UserWatcher : IUserWatcher
{
    private readonly UserWatcherSettings _options;
    public ProgramJob<string> Job { get; }

    public UserWatcher(IOptions<UserWatcherSettings> options, ILogger<UserWatcher> logger)
    {
        _options = options.Value;

        if (_options.CreateJob)
        {
            Job = new ProgramJob<string>(
                period: 5.Minutes(),
                method: DetectInactiveUsersAsync,
                description: InactiveUsersInformer,
                startUtcDate: DateTime.UtcNow + 5.Seconds(),
                logger: logger);
        }
    }

    private async Task<string> DetectInactiveUsersAsync()
    {
        var result = new StringBuilder();

        await foreach (var (user, inactiveTime) in GetUsersWithInactiveTimeAsync())
        {
            if (inactiveTime < _options.InactiveHoursLimit.Hours())
                continue;

            var userName = $"{user.FirstName} {user.LastName}";
            result.AppendLine($@"User {userName} is not active for {inactiveTime:hh\:mm\:ss}");
        }

        return result.ToString().Trim();
    }

    public async IAsyncEnumerable<(User, TimeSpan InactiveTime)> GetUsersWithInactiveTimeAsync()
    {
        foreach (var userId in _options.TrackedIds)
        {
            var inactiveTimeTask = GetInactiveTimeAsync(userId);
            var userTask = GetUserAsync(userId);
            await Task.WhenAll(inactiveTimeTask, userTask);

            if (userTask.Result != null)
                yield return (userTask.Result, inactiveTimeTask.Result);
        }
    }

    private async Task<User?> GetUserAsync(int userId)
    {
        var url = $"{_options.VkActivityApiUri}/api/users/{userId}";
        var user = await Request.Create(url).GetAsync<User>();
        return user;
    }

    private async Task<TimeSpan> GetInactiveTimeAsync(int userId)
    {
        var url = $"{_options.VkActivityApiUri}/api/activity/{userId}/last-utc";
        var lastSeen = await Request.Create(url).GetAsync<DateTime>();
        var inactiveTime = DateTime.UtcNow - lastSeen;
        return inactiveTime;
    }

    public async Task<string> GetCurrentStateAsync(TimeSpan? timeout = null)
    {
        var result = new StringBuilder();
        foreach (var userId in _options.TrackedIds)
        {
            var inactiveTime = await GetInactiveTimeAsync(userId);
            var user = await GetUserAsync(userId);
            var userName = $"{user!.FirstName} {user.LastName}";
            result.AppendLine($@"User {userName} is not active for {inactiveTime:hh\:mm\:ss}");
        }

        return result.ToString().Trim();
    }
}
