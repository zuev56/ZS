using System.ComponentModel.DataAnnotations;
using Zs.Home.Jobs.Hangfire.Hangfire;

namespace Zs.Home.Jobs.Hangfire.HardwareAnalyzer;

public sealed class HardwareAnalyzerSettings : ICronSettings
{
    internal const string SectionName = "HardwareAnalyzer";

    [Required]
    public required string CronExpression { get; init; }
}
