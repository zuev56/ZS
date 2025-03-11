using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zs.VkActivity.WebApi;

namespace Zs.Home.Application.Features.VkUsers;

public sealed class UserWatcher : IUserWatcher
{
    private readonly IActivityLogClient _activityLogClient;
    private readonly IUsersClient _usersClient;

    public UserWatcher(
        IActivityLogClient activityLogClient,
        IUsersClient usersClient)
    {
        _activityLogClient = activityLogClient;
        _usersClient = usersClient;
    }

    public async Task<IReadOnlyDictionary<User, TimeSpan>> GetUsersWithInactiveTimeAsync(
        int[] userIds, CancellationToken cancellationToken)
    {
        var inactiveUsers = new Dictionary<User, TimeSpan>();
        foreach (var userId in userIds)
        {
            var inactiveTimeTask = GetInactiveTimeAsync(userId, cancellationToken);
            var userTask = GetUserAsync(userId, cancellationToken);
            await Task.WhenAll(inactiveTimeTask, userTask);

            if (userTask.Result != null)
                inactiveUsers.Add(userTask.Result, inactiveTimeTask.Result);
        }

        return inactiveUsers;
    }

    private async Task<User?> GetUserAsync(int userId, CancellationToken cancellationToken)
    {
        var userDto = await _usersClient.GetUserAsync(userId, cancellationToken);
        return new User
        {
            Id = userDto.Id,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName
        };
    }

    private async Task<TimeSpan> GetInactiveTimeAsync(int userId, CancellationToken cancellationToken)
    {
        var lastSeen = await _activityLogClient.GetLastVisitDateAsync(userId, cancellationToken);
        var inactiveTime = DateTime.UtcNow - lastSeen;
        return inactiveTime;
    }
}
