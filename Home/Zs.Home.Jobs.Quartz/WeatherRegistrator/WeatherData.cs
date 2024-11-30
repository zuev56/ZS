using System;

namespace Zs.Home.Jobs.WeatherRegistrator;

public sealed class WeatherData
{
    public required int Id { get; set; }
    public required int PlaceId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public double? Temperature { get; set; }
    public double? Humidity { get; set; }
    public double? Pressure { get; set; }
    public double? CO2 { get; set; }
}
