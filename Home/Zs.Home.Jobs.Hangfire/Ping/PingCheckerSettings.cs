using System.ComponentModel.DataAnnotations;
using Zs.Home.Jobs.Hangfire.Hangfire;

namespace Zs.Home.Jobs.Hangfire.Ping;

public sealed class PingCheckerSettings : ICronSettings
{
    internal const string SectionName = "PingChecker";

    [Required]
    public required string CronExpression { get; init; }
}
