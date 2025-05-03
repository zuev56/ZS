using Zs.Parser.EspMeteo.Models;

namespace Zs.Home.WebApi.Features.Weather;

/// <summary>
/// Содержит информацию об отклонении значений от нормы
/// </summary>
public sealed record Deviation
{
    public required string DeviceName { get; init; }
    public string? SensorAlias { get; init; }
    public required string SensorName { get; init; }
    public required Parameter Parameter { get; init; }
    public required ParameterSettings Settings { get; init; }
    public DeviationType Type { get; init; }
}
