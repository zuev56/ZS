using System.ComponentModel.DataAnnotations;
using Zs.Home.Jobs.Hangfire.Hangfire;

namespace Zs.Home.Jobs.Hangfire.UserWatcher;

public sealed class UserWatcherSettings : Zs.Home.Application.Features.VkUsers.UserWatcherSettings, ICronSettings
{
    [Required]
    public required string CronExpression { get; init; }
}
