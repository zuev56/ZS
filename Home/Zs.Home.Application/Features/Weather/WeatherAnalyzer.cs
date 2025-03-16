using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Parser.EspMeteo;
using Zs.Parser.EspMeteo.Models;

namespace Zs.Home.Application.Features.Weather;

internal sealed class WeatherAnalyzer : IWeatherAnalyzer
{
    private readonly EspMeteoParser _espMeteoParser;
    private readonly ILogger<WeatherAnalyzer>? _logger;

    public WeatherAnalyzer(
        EspMeteoParser espMeteoParser,
        ILogger<WeatherAnalyzer>? logger)
    {
        _espMeteoParser = espMeteoParser;
        _logger = logger;
    }

    public async Task<string> GetDeviationInfosAsync(IReadOnlyList<DeviceSettings> deviceSettings, CancellationToken ct)
    {
        var espMeteos = await GetEspMeteoInfosAsync(deviceSettings, CancellationToken.None);
        var allDeviations = new StringBuilder();
        foreach (var espMeteo in espMeteos)
        {
            var settings = deviceSettings.Single(s => s.Uri == espMeteo.Uri);
            var deviations = AnalyzeDeviations(espMeteo, settings);
            if (string.IsNullOrEmpty(deviations))
                continue;

            allDeviations.AppendLine();
            allDeviations.AppendLine(deviations);
        }

        return allDeviations.ToString().Trim();
    }

    private async Task<EspMeteo[]> GetEspMeteoInfosAsync(IReadOnlyList<DeviceSettings> deviceSettings, CancellationToken ct)
    {
        var parseTasks = deviceSettings
            .Select(static d => d.Uri)
            .Select(url => _espMeteoParser.ParseAsync(url, ct));

        return await Task.WhenAll(parseTasks);
    }

    private static string AnalyzeDeviations(EspMeteo espMeteo, DeviceSettings deviceSettings)
    {
        var espMeteoDeviceName = $"[{deviceSettings.Name ?? espMeteo.Uri}].";
        var deviations = new StringBuilder();

        foreach (var sensor in espMeteo.Sensors)
        {
            var sensorSettings = deviceSettings.Sensors.SingleOrDefault(s => s.Name == sensor.Name);
            if (sensorSettings is null)
                continue;

            foreach (var parameter in sensor.Parameters)
            {
                var parameterSettings = sensorSettings.Parameters.SingleOrDefault(s => s.Name == parameter.Name);
                if (parameterSettings is null)
                    continue;

                if (parameter.Value > parameterSettings.HiHi)
                {
                    var deviation = $"{sensorSettings.Alias ?? sensor.Name}.{parameter.Name}: value {parameter.Value} {parameter.Unit} is higher than limit {parameterSettings.HiHi} {parameter.Unit}";
                    deviations.AppendLine(deviation);
                }

                if (parameter.Value < parameterSettings.LoLo)
                {
                    var deviation = $"{sensorSettings.Alias ?? sensor.Name}.{parameter.Name}: value {parameter.Value} {parameter.Unit} is lower than limit {parameterSettings.LoLo} {parameter.Unit}";
                    deviations.AppendLine(deviation);
                }
            }
        }

        var result = deviations.ToString();
        return result == espMeteoDeviceName ? string.Empty : result;
    }
}
