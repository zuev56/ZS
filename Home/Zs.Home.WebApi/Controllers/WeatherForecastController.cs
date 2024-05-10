using Microsoft.AspNetCore.Mvc;

namespace Zs.Home.WebApi.Controllers;

/*
 * Weather
 *      EspMeteoStatus(IP)
 *      -Temperature
 *      -Humidity
 *      -Pressure
 *      Forecast|CurrentOutside
 *
 * Ping
 *      IP
 *      IP:Port
 *
 * Seq
 *      Weeek
 *      24 hours
 *      12 hours
 *      6 hours
 *      Last hour
 *
 * Hardware|OS
 *      CPU temperature
 *      CPU usage
 *      Memory usage
 *		Journal analyzis
 *
 * Services
 *      HealthCheck
 *      Stop
 *      Start
 *      Restart
 *
 * Network
 *      Device list
 *      Unknown devices
 *
 */


[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
}