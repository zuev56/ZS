﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var userIds = await _usersRepo.FindAllIdsAsync().ConfigureAwait(false);

            if (!userIds.Any())
            {
                return new Fault(NoUsersInDatabase);
                //result.AddMessage(NoUsersInDatabase, InfoMessageType.Warning);
                //return result;
            }

            var userStringIds = userIds.Select(id => id.ToString()).ToArray();
            var vkUsers = await _vkIntegration.GetUsersWithActivityInfoAsync(userStringIds).ConfigureAwait(false);

            var loggedItemsCount = await LogVkUsersActivityAsync(vkUsers).ConfigureAwait(false);

#if DEBUG
            Trace.WriteLine(LoggedItemsCount(loggedItemsCount));
#endif

            //result.AddMessage(LoggedItemsCount(loggedItemsCount));
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogTraceIfNeed("Code: {Code}, Exception: {ExceptionType}, Message: {ExceptionMessage}", SaveUsersActivityError, ex.GetType().Name, ex.Message);
            _delayedLogger.LogError(SaveUsersActivityError);

            await ChangeAllUserActivitiesToUndefinedAsync().ConfigureAwait(false);

            return Result.Fail(SaveUsersActivityError);
        }
    }

    /// <summary>Save undefined user activities to database</summary>
    public async Task<Result> ChangeAllUserActivitiesToUndefinedAsync()
    {
        try
        {
            var users = await _usersRepo.FindAllAsync().ConfigureAwait(false);

            var lastUsersActivityLogItems = await _activityLogRepo.FindLastUsersActivityAsync();
            if (!lastUsersActivityLogItems.Any())
            {
                return Result.Fail(ActivityLogIsEmpty);
            }

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

#if DEBUG
            Trace.WriteLine(SetUndefinedActivityToAllUsers);
#endif
            return Result.Success();
        }
        catch
        {
            _logger.LogErrorIfNeed(SetUndefinedActivityToAllUsersError);
            return Result.Fail(SetUndefinedActivityToAllUsersError);
        }
    }

    /// <summary>Save user activities to database</summary>
    /// <param name="apiUsers">All users current state from VK API</param>
    /// <returns>Logged <see cref="ActivityLogItem"/>s count</returns>
    private async Task<int> LogVkUsersActivityAsync(List<VkApiUser> apiUsers)
    {
        // TODO: Add user activity info (range) - ???
        var lastActivityLogItems = await _activityLogRepo.FindLastUsersActivityAsync().ConfigureAwait(false);
        var activityLogItemsForSave = new List<ActivityLogItem>();

        foreach (var apiUser in apiUsers)
        {
            // When account is deleted or banned or smth else
            if (apiUser.LastSeen == null)
            {
                continue;
            }

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
                {
                    lastSeenForLog = Math.Max(lastActivityLogItem.LastSeen, apiUser.LastSeen.UnixTime);
                }

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
        {
            return 0;
        }

        return await _activityLogRepo.SaveRangeAsync(activityLogItemsForSave).ConfigureAwait(false)
            ? activityLogItemsForSave.Count
            : -1;
    }
}