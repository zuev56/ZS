using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Jobs.Hangfire.Ping;

public sealed class PingCheckerSettings
{
    public const string SectionName = "PingChecker";

    [Required]
    public required string CronExpression { get; set; }
}
