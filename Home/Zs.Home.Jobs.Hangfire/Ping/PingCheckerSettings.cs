using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Jobs.Hangfire.Ping;

public sealed class PingCheckerSettings : Zs.Home.Application.Features.Ping.PingCheckerSettings
{
    [Required]
    public required string CronExpression { get; set; }
}
