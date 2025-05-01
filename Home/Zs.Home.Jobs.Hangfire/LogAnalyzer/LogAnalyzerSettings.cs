using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Jobs.Hangfire.LogAnalyzer;

public sealed class LogAnalyzerSettings
{
    public const string SectionName = "LogAnalyzer";

    [Required]
    public required string CronExpression { get; set; }
}
