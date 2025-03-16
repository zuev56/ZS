using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Common.Extensions;
using Zs.Home.Application.Features.Weather;
using Zs.Home.Jobs.Hangfire.Extensions;
using Zs.Home.Jobs.Hangfire.Notification;

namespace Zs.Home.Jobs.Hangfire.WeatherAnalyzer;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class WeatherAnalyzerJob
{
    private static DateTime? _lastAlarmUtcDate = DateTime.UtcNow - 2.Hours();
    private static readonly TimeSpan _alarmInterval = 2.Hours();

    private readonly IWeatherAnalyzer _weatherAnalyzer;
    private readonly WeatherAnalyzerSettings _settings;
    private readonly Notifier _notifier;
    private readonly ILogger<WeatherAnalyzerJob> _logger;

    public WeatherAnalyzerJob(
        IWeatherAnalyzer weatherAnalyzer,
        IOptions<WeatherAnalyzerSettings> settings,
        Notifier notifier,
        ILogger<WeatherAnalyzerJob> logger)
    {
        _weatherAnalyzer = weatherAnalyzer;
        _notifier = notifier;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        if (DateTime.UtcNow < _lastAlarmUtcDate + _alarmInterval)
            return;

        var sw = Stopwatch.StartNew();
        _logger.LogJobStart();

        var weatherDeviations = await _weatherAnalyzer.GetDeviationInfosAsync(_settings.Devices, ct);

        await _notifier.SendNotificationAsync(weatherDeviations, ct);

        // TODO: Лучше возвращать и затем проверять результат отправки уведомления
        if (!string.IsNullOrEmpty(weatherDeviations))
            _lastAlarmUtcDate = DateTime.UtcNow;

        _logger.LogJobFinish(sw.Elapsed);
    }

}
