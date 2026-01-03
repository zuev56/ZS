using System.ComponentModel.DataAnnotations;
using Zs.Home.Jobs.Hangfire.Hangfire;

namespace Zs.Home.Jobs.Hangfire.LogAnalyzer;

public sealed class LogAnalyzerSettings : ICronSettings
{
    internal const string SectionName = "LogAnalyzer";

    [Required]
    public required string CronExpression { get; init; }
}
