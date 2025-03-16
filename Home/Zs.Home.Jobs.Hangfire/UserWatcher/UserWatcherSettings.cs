using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Jobs.Hangfire.UserWatcher;

public sealed class UserWatcherSettings : Zs.Home.Application.Features.VkUsers.UserWatcherSettings
{
    [Required]
    public required string CronExpression { get; set; }
}
