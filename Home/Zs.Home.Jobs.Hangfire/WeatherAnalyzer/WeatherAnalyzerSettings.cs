using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Jobs.Hangfire.WeatherAnalyzer;

public sealed class WeatherAnalyzerSettings
{
    public const string SectionName = "WeatherAnalyzer";

    [Required]
    public required string CronExpression { get; init; }
}
