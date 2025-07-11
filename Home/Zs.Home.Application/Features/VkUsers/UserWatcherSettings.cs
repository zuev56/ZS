using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Application.Features.VkUsers;

public class UserWatcherSettings
{
    public const string SectionName = "UserWatcher";

    [Required]
    public string VkActivityApiUri { get; set; } = null!;

    [Required]
    public int[] TrackedIds { get; set; } = [];

    [Required]
    public double InactiveHoursLimit { get; set; }
}
