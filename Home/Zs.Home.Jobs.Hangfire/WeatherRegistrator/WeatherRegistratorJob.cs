using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Common.Extensions;
using Zs.Home.Application.Features.Weather.Data;
using Zs.Home.Application.Features.Weather.Data.Models;
using Zs.Parser.EspMeteo;
using Zs.Parser.EspMeteo.Models;
using static Zs.Home.Jobs.Hangfire.Constants;

namespace Zs.Home.Jobs.Hangfire.WeatherRegistrator;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class WeatherRegistratorJob
{
    private readonly EspMeteoParser _espMeteoParser;
    private readonly WeatherRegistratorSettings _settings;
    private readonly IDbContextFactory<WeatherRegistratorDbContext> _dbContextFactory;
    private readonly ILogger<WeatherRegistratorJob> _logger;

    public WeatherRegistratorJob(
        IDbContextFactory<WeatherRegistratorDbContext> dbContextFactory,
        EspMeteoParser espMeteoParser,
        IOptions<WeatherRegistratorSettings> settings,
        ILogger<WeatherRegistratorJob> logger)
    {
        _dbContextFactory = dbContextFactory;
        _espMeteoParser = espMeteoParser;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task ExequteAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogDebugIfNeed("Start {Job}", nameof(WeatherRegistratorJob));

        var weatherData = await GetWeatherDataAsync(cancellationToken);

        await SaveWeatherDataAsync(weatherData, cancellationToken);
        _logger.LogDebugIfNeed("Finish {Job}, elapsed: {Elapsed}", nameof(WeatherRegistratorJob), sw.Elapsed);
    }

    private async Task<IReadOnlyList<WeatherData>> GetWeatherDataAsync(CancellationToken cancellationToken)
    {
        var espMeteos = await GetEspMeteoInfosAsync(cancellationToken);

        return espMeteos.SelectMany(
            espMeteo => espMeteo.Sensors.Select(sensor =>
            {
                var sensorSettings = GetSensorSettings(espMeteo, sensor);

                return new WeatherData
                {
                    SourceId = sensorSettings.Id,
                    Temperature = sensorSettings.Except?.Contains(Temperature) == true ? null
                        : sensor.Parameters.FirstOrDefault(p => p.Name == Temperature)?.Value,
                    Humidity = sensorSettings.Except?.Contains(Humidity) == true ? null
                        : sensor.Parameters.FirstOrDefault(p => p.Name == Humidity)?.Value,
                    Pressure = sensorSettings.Except?.Contains(Pressure) == true ? null
                        : sensor.Parameters.FirstOrDefault(p => p.Name == Pressure)?.Value,
                    CO2 = null
                };
            }))
            .ToImmutableList();
    }

    private Sensor GetSensorSettings(EspMeteo espMeteo, Parser.EspMeteo.Models.Sensor sensor)
        => _settings.Sensors.First(s => s.Uri == espMeteo.Uri && s.Name == sensor.Name);

    private async Task<IReadOnlyList<EspMeteo>> GetEspMeteoInfosAsync(CancellationToken cancellationToken)
    {
        var parseTasks = _settings.Sensors
            .Select(s => s.Uri)
            .Distinct()
            .Select(uri => _espMeteoParser.ParseAsync(uri, cancellationToken));

        return await Task.WhenAll(parseTasks);
    }

    private async Task SaveWeatherDataAsync(IReadOnlyList<WeatherData> weatherData, CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        context.WeatherData.AddRange(weatherData);

        await context.SaveChangesAsync(cancellationToken);
    }
}
