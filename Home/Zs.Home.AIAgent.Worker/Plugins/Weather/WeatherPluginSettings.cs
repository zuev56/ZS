using System.ComponentModel.DataAnnotations;
using Zs.Home.AIAgent.Worker.Models;

namespace Zs.Home.AIAgent.Worker.Plugins.Weather;

public sealed class WeatherPluginSettings
{
    public const string PluginName = "Weather";
    public const string SectionName = $"{Constants.Plugins}:{PluginName}";

    [Required]
    public required string ApiKey { get; set; }
    [Required]
    public required string BaseUrl { get; set; }
}
