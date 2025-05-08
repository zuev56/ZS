using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Logging.DelayedLogger;
using Zs.VkActivity.Common.Abstractions;
using Zs.VkActivity.Common.Models.VkApi;
using Zs.VkActivity.Data.Abstractions;
using Zs.VkActivity.Data.Models;
using Zs.VkActivity.Worker.Abstractions;
using static Zs.VkActivity.Worker.Models.Constants;

namespace Zs.VkActivity.Worker.Services;

public sealed class ActivityLogger : IActivityLogger
{
    private readonly IActivityLogItemsRepository _activityLogRepo;
    private readonly IUsersRepository _usersRepo;
    private readonly IVkIntegration _vkIntegration;
    private readonly ILogger<ActivityLogger> _logger;
    private readonly IDelayedLogger<ActivityLogger> _delayedLogger;

    public ActivityLogger(
        IActivityLogItemsRepository activityLogRepo,
        IUsersRepository usersRepo,
        IVkIntegration vkIntegration,
        ILogger<ActivityLogger> logger,
        IDelayedLogger<ActivityLogger> delayedLogger)
    {
        _activityLogRepo = activityLogRepo;
        _usersRepo = usersRepo;
        _vkIntegration = vkIntegration;
        _logger = logger;
        _delayedLogger = delayedLogger;
    }

    /// <inheritdoc/>
    public async Task<Result> SaveUsersActivityAsync()
    {
        try
        {
            var userIds = await _usersRepo.FindAllIdsAsync();

            if (!userIds.Any())
                return new Fault(NoUsersInDatabase);

            var userStringIds = userIds.Select(id => id.ToString()).ToArray();
            var vkUsers = await _vkIntegration.GetUsersWithActivityInfoAsync(userStringIds);

            var loggedItemsCount = await LogVkUsersActivityAsync(vkUsers);

            _logger.LogTraceIfNeed("LoggedItems: {Count}", loggedItemsCount);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogTraceIfNeed("Code: {Code}, Exception: {ExceptionType}, Message: {ExceptionMessage}", SaveUsersActivityError, ex.GetType().Name, ex.Message);
            _delayedLogger.LogError($"Code: {SaveUsersActivityError}, Exception: {ex.GetType().Name}, Message: {ex.Message}");

            await ChangeAllUserActivitiesToUndefinedAsync();

            return Result.Fail(SaveUsersActivityError);
        }
    }

    /// <summary>Save undefined user activities to database</summary>
    public async Task<Result> ChangeAllUserActivitiesToUndefinedAsync()
    {
        _logger.TraceMethod();

        try
        {
            var users = await _usersRepo.FindAllAsync();

            var lastUsersActivityLogItems = await _activityLogRepo.FindLastUsersActivityAsync();
            if (!lastUsersActivityLogItems.Any())
                return Result.Fail(ActivityLogIsEmpty);

            var activityLogItems = new List<ActivityLogItem>();
            foreach (var user in users)
            {
                var userActivityItem = lastUsersActivityLogItems.FirstOrDefault(i => i.UserId == user.Id);
                if (userActivityItem?.IsOnline != null)
                {
                    activityLogItems.Add(new ActivityLogItem
                    {
                        UserId = user.Id,
                        IsOnline = null,
                        Platform = 0,
                        LastSeen = userActivityItem.LastSeen,
                        InsertDate = DateTime.UtcNow
                    });
                }
            }

            await _activityLogRepo.SaveRangeAsync(activityLogItems);

            return Result.Success();
        }
        catch
        {
            _logger.LogErrorIfNeed(nameof(ChangeAllUserActivitiesToUndefinedAsync));
            return Result.Fail(nameof(ChangeAllUserActivitiesToUndefinedAsync));
        }
    }

    /// <summary>Save user activities to database</summary>
    /// <param name="apiUsers">All users current state from VK API</param>
    /// <returns>Logged <see cref="ActivityLogItem"/>s count</returns>
    private async Task<int> LogVkUsersActivityAsync(List<VkApiUser> apiUsers)
    {
        // TODO: Add user activity info (range) - ???
        var lastActivityLogItems = await _activityLogRepo.FindLastUsersActivityAsync();
        var activityLogItemsForSave = new List<ActivityLogItem>();

        foreach (var apiUser in apiUsers)
        {
            // When account is deleted or banned or smth else
            if (apiUser.LastSeen == null)
                continue;

            var lastActivityLogItem = lastActivityLogItems.FirstOrDefault(i => i.UserId == apiUser.Id);
            var currentPlatform = apiUser.LastSeen.Platform;
            var currentIsOnline = apiUser.IsOnline == 1;

            if (lastActivityLogItem == null
                || lastActivityLogItem.IsOnline != currentIsOnline
                || lastActivityLogItem.Platform != currentPlatform)
            {
                // Vk corrects LastSeen, so we have to work with logged value, not current API value
                var lastSeenForLog = apiUser.LastSeen?.UnixTime ?? 0;
                if (lastActivityLogItem != null && apiUser.LastSeen != null)
                    lastSeenForLog = Math.Max(lastActivityLogItem.LastSeen, apiUser.LastSeen.UnixTime);

                activityLogItemsForSave.Add(
                    new ActivityLogItem
                    {
                        UserId = apiUser.Id,
                        IsOnline = currentIsOnline,
                        Platform = currentPlatform,
                        LastSeen = lastSeenForLog,
                        InsertDate = DateTime.UtcNow
                    });
            }
        }

        if (!activityLogItemsForSave.Any())
            return 0;

        return await _activityLogRepo.SaveRangeAsync(activityLogItemsForSave)
            ? activityLogItemsForSave.Count
            : -1;
    }
}
