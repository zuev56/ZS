using System.Collections.Generic;
using System.Linq;
using Zs.Home.Application.Features.Weather.Data.Models;
using static Zs.Home.ClientApp.Pages.Dashboard.Constants;

namespace Zs.Home.ClientApp.Pages.Dashboard;

internal static class Mapper
{
    public static WeatherDashboard ToWeatherDashboard(this IReadOnlyList<WeatherData> weatherData, WeatherDashboardSettings settings)
    {
        var dbSources = weatherData
            .Select(d => d.Source)
            .DistinctBy(s => s.Id)
            .ToList();

        var dbPlaces = dbSources
            .Select(s => s.Place)
            .DistinctBy(p => p.Id)
            .ToList();

        // TODO: O(n^3)
        foreach (var place in dbPlaces)
        {
            place.Sources = dbSources.Where(s => s.PlaceId == place.Id).ToList();

            foreach (var source in place.Sources)
            {
                source.Place = place;
                source.WeatherData = weatherData.Where(d => d.SourceId == source.Id).ToList();

                foreach (var data in source.WeatherData)
                {
                    data.Source = source;
                }
            }
        }

        var clientPlaces = dbPlaces
            .Select(p => p.ToClientModel(settings))
            .ToList();

        return new WeatherDashboard { Places = clientPlaces };
    }

    private static Place ToClientModel(
        this Application.Features.Weather.Data.Models.Place place,
        WeatherDashboardSettings settings)
    {
        var parameters = place.Sources!.SelectMany(s => s.WeatherData!)
            .SelectMany(d =>
            {
                return new[]
                {
                    !d.Temperature.HasValue
                        ? (ParameterName: default, Value: default, Unit: default, CreatedAt: default)
                        : (ParameterName: Temperature, d.Temperature.Value, Unit: TemperatureUnit, d.CreatedAt),
                    !d.Humidity.HasValue
                        ? (ParameterName: default, Value: default, Unit: default, CreatedAt: default)
                        : (ParameterName: Humidity, d.Humidity.Value, Unit: HumidityUnit, d.CreatedAt),
                    !d.Pressure.HasValue
                        ? (ParameterName: default, Value: default, Unit: default, CreatedAt: default)
                        : (ParameterName: Pressure, d.Pressure.Value, Unit: PressureUnit, d.CreatedAt)
                    // !d.CO2.HasValue ? (default, default, default, default)
                    //     : new AnalogParameter(Co2, d.CO2.Value, Co2Unit, placeParameterMap[Co2])
                };
            })
            .GroupBy(a => new {a.ParameterName, a.Unit})
            .Where(g => g.Key.ParameterName != default)
            .Select(g =>
            {
                var valueLog = g.ToDictionary(i => i.CreatedAt, i => i.Value);

                var placeParameterMap = settings.Parameters
                    .Where(p => p.PlaceId == place.Id)
                    .ToDictionary(p => p.Name);

                return new AnalogParameter(g.Key.ParameterName!, valueLog, g.Key.Unit!, placeParameterMap[g.Key.ParameterName!]);
            })
            .ToList();

        return new Place
        {
            Name = place.Name,
            Parameters = parameters
        };
    }
}
