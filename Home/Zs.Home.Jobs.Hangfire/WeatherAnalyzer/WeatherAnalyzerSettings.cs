using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Jobs.Hangfire.WeatherAnalyzer;

public sealed class WeatherAnalyzerSettings : Zs.Home.Application.Features.Weather.WeatherAnalyzerSettings
{
    [Required]
    public required string CronExpression { get; set; }
}
