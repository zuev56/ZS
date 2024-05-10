using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zs.VkActivity.Data.Models;
using Zs.VkActivity.Data.Repositories;

namespace Zs.VkActivity.Data.Abstractions;

public interface IActivityLogItemsRepository : IBaseRepository<ActivityLogItem>
{
    Task<List<ActivityLogItem>> FindLastUsersActivityAsync(params int[] userIds);
    Task<List<ActivityLogItem>> FindAllByIdsInDateRangeAsync(int[] userIds, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
}
