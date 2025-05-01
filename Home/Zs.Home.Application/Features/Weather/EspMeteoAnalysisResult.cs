using System;
using System.Collections.Generic;
using System.Linq;
using Zs.Home.Application.Models;
using Zs.Parser.EspMeteo.Models;

namespace Zs.Home.Application.Features.Weather;

/// <summary>
/// Содержит все данные ESP Meteo + соответствующие настройки + отклонения.
/// </summary>
public sealed class EspMeteoAnalysisResult
{
    private readonly Dictionary<Parameter, Sensor> _parameterToSensorMap = new();
    private readonly Dictionary<Parameter, ParameterSettings> _parameterToSettingsMap = new();
    private readonly IReadOnlyList<Sensor> _sensors;
    private readonly DeviceSettings _settings;

    public string? Name => _settings.Name;
    public string Uri => _settings.Uri;
    public IReadOnlyList<Deviation> Deviations { get; private set; } = null!;

    public EspMeteoAnalysisResult(EspMeteo espMeteo, DeviceSettings settings)
    {
        _sensors = espMeteo.Sensors;
        _settings = settings;

        Validate(espMeteo, settings);

        CreateParameterMaps();

        CreateDeviations();
    }

    private static void Validate(EspMeteo espMeteo, DeviceSettings settings)
    {
        if (espMeteo.Uri != settings.Uri)
            throw new ArgumentException("ESP Meteo and settings URIs do not match.");

        foreach (var settingsSensor in settings.Sensors)
        {
            var espSensor = espMeteo.Sensors.SingleOrDefault(s => s.Name == settingsSensor.Name);
            if (espSensor == null)
                throw new ArgumentException("ESP Meteo and settings Sensors do not match.");

            if (settingsSensor.Parameters.Any(sp => espSensor.Parameters.SingleOrDefault(ep => ep.Name == sp.Name) == null))
                throw new ArgumentException("ESP Meteo sensor parameters and settings parameters do not match.");
        }
    }

    private void CreateParameterMaps()
    {
        foreach (var sensor in _sensors)
        {
            var sensorSettings = _settings.Sensors.SingleOrDefault(s => s.Name == sensor.Name);
            if (sensorSettings is null)
                continue;

            foreach (var parameter in sensor.Parameters)
            {
                var parameterSettings = sensorSettings.Parameters.SingleOrDefault(s => s.Name == parameter.Name);
                if (parameterSettings is null)
                    continue;

                _parameterToSensorMap[parameter] = sensor;
                _parameterToSettingsMap[parameter] = parameterSettings;
            }
        }
    }

    private void CreateDeviations()
    {
        var deviations = new List<Deviation>();

        foreach (var (parameter, parameterSettings) in _parameterToSettingsMap)
        {
            var deviation = parameter.Value switch
            {
                var value when value > parameterSettings.HiHi => CreateDeviation(parameter, DeviationType.HiHi),
                var value when value > parameterSettings.Hi => CreateDeviation(parameter, DeviationType.Hi),
                var value when value < parameterSettings.Lo => CreateDeviation(parameter, DeviationType.Lo),
                var value when value < parameterSettings.LoLo => CreateDeviation(parameter, DeviationType.LoLo),
                _ => null
            };

            if (deviation != null)
                deviations.Add(deviation);
        }

        Deviations = deviations;
    }

    private Deviation CreateDeviation(Parameter parameter, DeviationType deviationType)
    {
        var sensor = _parameterToSensorMap[parameter];
        var sensorSettings = _settings.Sensors.Single(s => s.Name == sensor.Name);
        var parameterSettings = _parameterToSettingsMap[parameter];

        return new Deviation
        {
            DeviceName = Name ?? Uri,
            SensorAlias = sensorSettings.Alias,
            SensorName = sensor.Name,
            Parameter = parameter,
            Settings = parameterSettings,
            Type = deviationType
        };
    }
}
