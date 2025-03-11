using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Zs.Home.Application.Features.VkUsers;

public interface IUserWatcher
{
    Task<IReadOnlyDictionary<User, TimeSpan>> GetUsersWithInactiveTimeAsync(
        int[] userIds, CancellationToken cancellationToken);
}
