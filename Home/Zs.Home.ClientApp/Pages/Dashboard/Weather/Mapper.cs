using System.Collections.Generic;
using System.Linq;
using Zs.Home.Application.Features.Weather.Data.Models;
using Zs.Home.WebApi;
using static Zs.Home.ClientApp.Pages.Dashboard.Constants;

namespace Zs.Home.ClientApp.Pages.Dashboard.Weather;

internal static class Mapper
{
    public static WeatherDashboard ToWeatherDashboard(this IReadOnlyList<WeatherData> weatherData, GetWeatherAnalysisSettingsResponse settings)
    {
        var dbSources = weatherData
            .Select(d => d.Source)
            .DistinctBy(s => s.Id)
            .ToList();

        var dbPlaces = dbSources
            .Select(s => s.Place)
            .DistinctBy(p => p.Id)
            .ToList();

        foreach (var place in dbPlaces)
        {
            place.Sources = dbSources.Where(s => s.PlaceId == place.Id).ToList();

            foreach (var source in place.Sources)
            {
                source.Place = place;
                source.WeatherData = weatherData.Where(d => d.SourceId == source.Id).ToList();

                foreach (var data in source.WeatherData)
                    data.Source = source;
            }
        }

        var clientPlaces = dbPlaces
            .Select(p =>
            {
                // TODO: сделать возможность маппить по ID!
                var sensorSettings = settings.DeviceSettings.SelectMany(s => s.Sensors).Where(s => s.Alias == p.Name).ToList();
                return p.ToClientPlaceModel(sensorSettings);
            })
            .ToList();

        return new WeatherDashboard { Places = clientPlaces };
    }

    private static Place ToClientPlaceModel(
        this Application.Features.Weather.Data.Models.Place place,
        IReadOnlyList<SensorSettings> sensorSettings)
    {
        var parameterSettings = sensorSettings.SelectMany(ss => ss.Parameters);
        var parameters = place.Sources!.SelectMany(s => s.WeatherData!)
            .SelectMany(d =>
            {
                return new[]
                {
                    !d.Temperature.HasValue
                        ? (ParameterName: null, Value: 0, Unit: null, CreatedAt: default)
                        : (ParameterName: Temperature, d.Temperature.Value, Unit: TemperatureUnit, d.CreatedAt),
                    !d.Humidity.HasValue
                        ? (ParameterName: null, Value: 0, Unit: null, CreatedAt: default)
                        : (ParameterName: Humidity, d.Humidity.Value, Unit: HumidityUnit, d.CreatedAt),
                    !d.Pressure.HasValue
                        ? (ParameterName: null, Value: 0, Unit: null, CreatedAt: default)
                        : (ParameterName: Pressure, d.Pressure.Value, Unit: PressureUnit, d.CreatedAt)
                    // !d.CO2.HasValue ? (default, default, default, default)
                    //     : new AnalogParameter(Co2, d.CO2.Value, Co2Unit, placeParameterMap[Co2])
                };
            })
            .GroupBy(a => new {a.ParameterName, a.Unit})
            .Where(g => g.Key.ParameterName != null && parameterSettings.Any(p => p.Name == g.Key.ParameterName))
            .Select(g =>
            {
                var paramSettings = parameterSettings.Single(p => p.Name == g.Key.ParameterName);
                var valueLog = g.ToDictionary(i => i.CreatedAt, i => new LogValue(i.Value, GetStatus(i.Value, paramSettings)));

                return new AnalogParameter(g.Key.ParameterName!, valueLog, g.Key.Unit!, paramSettings);
            })
            //.OrderBy(p => p.Name)
            .ToList();

        return new Place
        {
            Name = place.Name,
            Parameters = parameters
        };
    }

    private static Status GetStatus(double value, ParameterSettings settings)
    {
        var (lolo, lo, hi, hihi) = (settings.LoLo, settings.Lo, settings.Hi, settings.HiHi);
        if (lolo < value && value < lo) return Status.WarningLo;
        if (hi < value && value < hihi) return Status.WarningHi;
        if (lolo > value) return Status.DangerLoLo;
        if (value > hihi) return Status.DangerHiHi;

        return Status.Normal;
    }
}
