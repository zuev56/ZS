using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;
using Zs.Home.Jobs.Hangfire.Extensions;
using Zs.Home.Jobs.Hangfire.Notification;
using Zs.Home.WebApi;

namespace Zs.Home.Jobs.Hangfire.WeatherAnalyzer;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class WeatherAnalyzerJob
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
                    DeviationType.HiHi => $"is higher than {settings.HiHi} {parameter.Unit} ({DeviationType.HiHi})",
                    DeviationType.Hi => $"is higher than {settings.Hi} {parameter.Unit} ({DeviationType.Hi})",
                    DeviationType.Lo => $"is lower than {settings.Lo} {parameter.Unit} ({DeviationType.Lo})",
                    DeviationType.LoLo => $"is lower than {settings.LoLo} {parameter.Unit} ({DeviationType.LoLo})",
                    _ => throw new ArgumentOutOfRangeException()
                };

                message.AppendLine(
                    $"{deviation.SensorAlias ?? deviation.SensorName}" +
                    $".{parameter.Name}: {parameter.Value} {parameter.Unit} {comparison}");
            }
        }

        return message.ToString();
    }
}
