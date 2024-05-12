using System;
using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Application.Features.Weather;

public sealed class WeatherAnalyzerSettings
{
    public const string SectionName = "WeatherAnalyzer";
    [Required]
    public DeviceSettings[] Devices { get; init; } = Array.Empty<DeviceSettings>();
}

public sealed class DeviceSettings
{
    [Required, Url]
    public string Uri { get; init; } = null!;
    [Required]
    public SensorSettings[] Sensors { get; init; } = Array.Empty<SensorSettings>();
    public string? Name { get; init; }
}

public sealed class SensorSettings
{
    [Required]
    public string Name { get; init; } = null!;
    public string? Alias { get; init; }
    [Required]
    public ParameterSettings[] Parameters { get; init; } = Array.Empty<ParameterSettings>();
}

public sealed class ParameterSettings
{
    [Required]
    public string Name { get; init; } = null!;
    [Required]
    public float HighLimit { get; init; }
    [Required]
    public float LowLimit { get; init; }
}
