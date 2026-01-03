using System;

namespace Zs.Home.Application.Features.Weather.Data.Models;

public sealed class WeatherData
{
    public int Id { get; set; }
    public required short SourceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public double? Temperature { get; set; }
    public double? Humidity { get; set; }
    public double? Pressure { get; set; }
    public double? CO2 { get; set; }

    public Source Source { get; set; } = null!;
}
