using System.ComponentModel;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace Zs.Home.AIAgent.Worker.Plugins.Weather;

public sealed class WeatherPlugin
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherPlugin> _logger;
    private readonly string _apiKey;

    public WeatherPlugin(HttpClient httpClient, IOptions<WeatherPluginSettings> settings, ILogger<WeatherPlugin> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = settings.Value.ApiKey;
        _httpClient.BaseAddress = new Uri(settings.Value.BaseUrl);

        _logger.LogInformation("Weather plugin loaded");
    }

    [KernelFunction("get_weather_for_city")]
    [Description("Получает текущую погоду для указанного города.")]
    public async Task<string> GetWeatherForCityAsync(
        [Description("Название города, для которого нужно получить погоду (например, 'London' или 'Петрозаводск'")]
        string city)
    {
        Console.Write($" Использую функцию get_weather_for_city(city: {city}).");

        var requestUrl = $"weather?q={city}&appid={_apiKey}&units=metric&lang=ru";

        try
        {
            var weatherData = await _httpClient.GetFromJsonAsync<OpenWeatherResponse>(requestUrl);

            if (weatherData?.Main != null)
            {
                return $"В городе {city} сейчас {weatherData.Weather?.FirstOrDefault()?.Description}. " +
                       $"Температура: {weatherData.Main.Temp}°C, ощущается как {weatherData.Main.FeelsLike}°C. " +
                       $"Влажность: {weatherData.Main.Humidity}%.";
            }

            return $"Не удалось получить погоду для города '{city}'.";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, message: null);
            return $"Произошла ошибка при запросе погоды для города '{city}'";
        }
    }
}
