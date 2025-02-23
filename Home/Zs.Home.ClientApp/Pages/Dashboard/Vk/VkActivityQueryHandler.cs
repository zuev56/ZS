using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using Zs.Home.Application.Features.VkUsers;

namespace Zs.Home.ClientApp.Pages.Dashboard.Vk;

public sealed class VkActivityQueryHandler : IRequestHandler<VkActivityQuery, VkActivity>
{
    private readonly IUserWatcher _userWatcher;
    private readonly UserWatcherSettings _options;

    public VkActivityQueryHandler(IUserWatcher userWatcher, IOptions<UserWatcherSettings> options)
    {
        _userWatcher = userWatcher;
        _options = options.Value;
    }

    public async Task<VkActivity> Handle(VkActivityQuery request, CancellationToken cancellationToken)
    {
        var users = new List<User>();

        await foreach (var (user, inactiveTime) in _userWatcher.GetUsersWithInactiveTimeAsync().WithCancellation(cancellationToken))
        {
            var name = $"{user.FirstName} {user.LastName}";
            var lastActivity = inactiveTime.TotalHours < _options.InactiveHoursLimit
                ? LastActivity.Recent
                : inactiveTime.TotalHours < _options.InactiveHoursLimit * 2
                    ? LastActivity.LongAgo
                    : LastActivity.TooLongAgo;

            users.Add(new User(name, inactiveTime, lastActivity));
        }

        return new VkActivity { Users = users };
    }
}
