using System.Text.Json.Serialization;

namespace Zs.Home.AIAgent.Worker.Plugins.Weather;

public sealed class OpenWeatherResponse
{
    [JsonPropertyName("main")]
    public MainData? Main { get; set; }
    [JsonPropertyName("weather")]
    public List<WeatherDescription>? Weather { get; set; }
}

public sealed class MainData
{
    [JsonPropertyName("temp")]
    public double Temp { get; set; }
    [JsonPropertyName("feels_like")]
    public double FeelsLike { get; set; }
    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}

public sealed class WeatherDescription
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
