using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;
using Zs.Home.Jobs.Hangfire.Extensions;
using Zs.Home.Jobs.Hangfire.Hangfire;
using Zs.Home.Jobs.Hangfire.Notification;
using Zs.Home.WebApi;

namespace Zs.Home.Jobs.Hangfire.WeatherAnalyzer;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class WeatherAnalyzerJob : IJob
{
    private static readonly TimeSpan _alarmInterval = 2.Hours();
    private static DateTime? _lastAlarmUtcDate = DateTime.UtcNow - _alarmInterval;

    private readonly IWeatherClient _weatherClient;
    private readonly Notifier _notifier;
    private readonly ILogger<WeatherAnalyzerJob> _logger;

    public WeatherAnalyzerJob(
        IWeatherClient weatherClient,
        Notifier notifier,
        ILogger<WeatherAnalyzerJob> logger)
    {
        _weatherClient = weatherClient;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        if (DateTime.UtcNow < _lastAlarmUtcDate + _alarmInterval)
            return;

        var sw = Stopwatch.StartNew();
        _logger.LogJobStart();

        var fullAnalysisResponse = await _weatherClient.GetFullAnalysisAsync(ct);

        var message = GetWeatherDeviationsMessage(fullAnalysisResponse.EspMeteoAnalysisResults);

        await _notifier.SendNotificationAsync(message, ct);

        // TODO: Лучше возвращать и затем проверять результат отправки уведомления
        //       (Что возвращать?)
        if (!string.IsNullOrEmpty(message))
            _lastAlarmUtcDate = DateTime.UtcNow;

        _logger.LogJobFinish(sw.Elapsed);
    }

    private static string GetWeatherDeviationsMessage(IList<EspMeteoAnalysisResult> espMeteoAnalysisResults)
    {
        var message = new StringBuilder();
        foreach (var analysisResult in espMeteoAnalysisResults)
        {
            foreach (var deviation in analysisResult.Deviations)
            {
                var parameter = deviation.Parameter;
                var settings = deviation.Settings;
                var comparison = deviation.Type switch
                {
                    DeviationType.HiHi => $"> {settings.HiHi} ({DeviationType.HiHi})",
                    DeviationType.Hi => $"> {settings.Hi} ({DeviationType.Hi})",
                    DeviationType.Lo => $"< {settings.Lo} ({DeviationType.Lo})",
                    DeviationType.LoLo => $"< {settings.LoLo} ({DeviationType.LoLo})",
                    _ => throw new ArgumentOutOfRangeException()
                };

                message.AppendLine(
                    $"{deviation.SensorAlias ?? deviation.SensorName}" +
                    $" {GetParameterLetterOrName(parameter.Name)} {parameter.Value} {parameter.Unit} {comparison}");
            }
        }

        return message.ToString();
    }

    private static string GetParameterLetterOrName(string parameter)
        => parameter.ToLowerInvariant() switch
        {
            "temperature" => "T =",
            "humidity" => "\ud835\udf11 =",
            "pressure" => "p =",
            _ => $"{parameter}:"
        };
}
