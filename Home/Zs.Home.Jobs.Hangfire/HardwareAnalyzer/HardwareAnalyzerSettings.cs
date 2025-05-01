using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Jobs.Hangfire.HardwareAnalyzer;

public sealed class HardwareAnalyzerSettings
{
    public const string SectionName = "HardwareAnalyzer";

    [Required]
    public required string CronExpression { get; set; }
}
