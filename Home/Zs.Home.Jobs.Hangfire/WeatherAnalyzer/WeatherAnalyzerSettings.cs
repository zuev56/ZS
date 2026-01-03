using System.ComponentModel.DataAnnotations;
using Zs.Home.Jobs.Hangfire.Hangfire;

namespace Zs.Home.Jobs.Hangfire.WeatherAnalyzer;

public sealed class WeatherAnalyzerSettings : ICronSettings
{
    internal const string SectionName = "WeatherAnalyzer";

    [Required]
    public required string CronExpression { get; init; }
}
