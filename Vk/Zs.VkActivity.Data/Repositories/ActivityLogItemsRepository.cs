﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;
using Zs.VkActivity.Data.Abstractions;
using Zs.VkActivity.Data.Models;

namespace Zs.VkActivity.Data.Repositories;

public sealed class ActivityLogItemsRepository : BaseRepository<VkActivityContext, ActivityLogItem>, IActivityLogItemsRepository
{
    public ActivityLogItemsRepository(
        IDbContextFactory<VkActivityContext> contextFactory,
        TimeSpan? criticalQueryExecutionTimeForLogging = null,
        ILoggerFactory? loggerFactory = null)
        : base(contextFactory, criticalQueryExecutionTimeForLogging, loggerFactory)
    {
    }

    public async Task<List<ActivityLogItem>> FindAllByIdsInDateRangeAsync(int[] userIds, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await FindAllAsync(
            l => userIds.Contains(l.UserId)
                && l.LastSeen >= fromDate.ToUnixEpoch()
                && l.LastSeen <= toDate.ToUnixEpoch(),
            cancellationToken: cancellationToken);
    }

    public async Task<List<ActivityLogItem>> FindLastUsersActivityAsync(params int[] userIds)
    {
        var sql = @"WITH RECURSIVE t AS ( 
                          (SELECT * FROM vk.activity_log ORDER BY user_id DESC, last_seen DESC, insert_date DESC LIMIT 1)
                          UNION ALL SELECT bpt.* FROM t, 
                          LATERAL (SELECT * FROM vk.activity_log WHERE user_id < t.user_id ORDER BY user_id DESC, last_seen DESC, insert_date DESC LIMIT 1) AS bpt 
                        ) SELECT * FROM t";

        if (userIds.Length > 0)
        {
            sql += $" WHERE t.user_id in ({string.Join(',', userIds)})";
        }

        return await FindAllBySqlAsync(sql);
    }
}