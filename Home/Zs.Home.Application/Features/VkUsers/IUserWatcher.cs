using System;
using System.Collections.Generic;
using Zs.Home.Application.Features.Hardware;

namespace Zs.Home.Application.Features.VkUsers;

public interface IUserWatcher : IHasJob, IHasCurrentState
{
    IAsyncEnumerable<(User, TimeSpan InactiveTime)> GetUsersWithInactiveTimeAsync();
}
