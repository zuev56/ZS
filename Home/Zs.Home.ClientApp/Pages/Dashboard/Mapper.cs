using System.Collections.Generic;
using System.Linq;
using Zs.Home.Application.Features.Weather.Data.Models;
using static Zs.Home.ClientApp.Pages.Dashboard.Constants;

namespace Zs.Home.ClientApp.Pages.Dashboard;

internal static class Mapper
{
    public static WeatherDashboard ToWeatherDashboard(this List<WeatherData> weatherData, WeatherDashboardSettings settings)
    {
        var places = weatherData.Select(d => d.Source.Place!)
            .Select(p => ToClientModel(p, settings))
            .ToList();

        // Если два датчика находятся в одном месте, то может получиться несколько одноимённых параметров.
        // Надо подумать, как исключить ненужный. Вероятно, его просто не надо писть в БД.
        // В ESPMeteo есть датчик температуры, который показывает t устройства, а не воздуха
        places = places
            .GroupBy(p => p.Name)
            .Select(g =>
            {
                var firstPlace = g.First();
                if (g.Count() > 1)
                    firstPlace.Parameters = g.SelectMany(p => p.Parameters).ToList();

                return firstPlace;
            }).ToList();

        return new WeatherDashboard { Places = places };
    }

    private static Place ToClientModel(
        this Application.Features.Weather.Data.Models.Place place, WeatherDashboardSettings settings)
    {
        var parameters = place.Sources!.SelectMany(s => s.WeatherData!)
            .SelectMany(d =>
            {
                var placeParameterMap = settings.Parameters
                    .Where(p => p.PlaceId == d.Source.PlaceId)
                    .ToDictionary(p => p.Name);

                return new[]
                {
                    !d.Temperature.HasValue ? null!
                        : new AnalogParameter(Temperature, d.Temperature.Value, TemperatureUnit, placeParameterMap[Temperature]),
                    !d.Humidity.HasValue ? null!
                        : new AnalogParameter(Humidity, d.Humidity.Value, HumidityUnit, placeParameterMap[Humidity]),
                    !d.Pressure.HasValue ? null!
                        : new AnalogParameter(Pressure, d.Pressure.Value, PressureUnit, placeParameterMap[Pressure]),
                    // !d.CO2.HasValue ? null
                    //     : new AnalogParameter(Co2, d.CO2.Value, Co2Unit, placeParameterMap[Co2])
                };
            })
            .Where(p => p != null!)
            .ToList();

        return new Place
        {
            Name = place.Name,
            Parameters = parameters
        };
    }
}
