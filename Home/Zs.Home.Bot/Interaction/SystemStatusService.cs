using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Zs.Common.Models;
using Zs.Home.Application.Features.Hardware;
using Zs.Home.Application.Features.Seq;
using Zs.Home.Application.Features.VkUsers;
using Zs.Home.Application.Features.Weather;
using Zs.Home.WebApi;
using Zs.Parser.EspMeteo;

namespace Zs.Home.Bot.Interaction;

internal sealed class SystemStatusService
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(1);

    private readonly IHardwareMonitor _hardwareMonitor;
    private readonly IUserWatcher _userWatcher;
    private readonly EspMeteoParser _espMeteoParser;
    private readonly IPingClient _pingClient;
    private readonly ILogAnalyzer _logAnalyzer;
    private readonly UserWatcherSettings _userWatcherSettings;
    private readonly WeatherAnalyzerSettings _weatherAnalyzerSettings;
    private readonly SeqSettings _seqSettings;

    public SystemStatusService(
        IHardwareMonitor hardwareMonitor,
        IUserWatcher userWatcher,
        EspMeteoParser espMeteoParser,
        IPingClient pingClient,
        ILogAnalyzer logAnalyzer,
        IOptions<UserWatcherSettings> userWatcherSettings,
        IOptions<WeatherAnalyzerSettings> weatherAnalyzerSettings,
        IOptions<SeqSettings> seqSettings)
    {
        _hardwareMonitor = hardwareMonitor;
        _userWatcher = userWatcher;
        _pingClient = pingClient;
        _logAnalyzer = logAnalyzer;
        _espMeteoParser = espMeteoParser;
        _userWatcherSettings = userWatcherSettings.Value;
        _weatherAnalyzerSettings = weatherAnalyzerSettings.Value;
        _seqSettings = seqSettings.Value;
    }

    public async Task<string> GetFullStatus()
    {
        var sw = Stopwatch.StartNew();
        var getHardwareStatus = GetHardwareStatusAsync();
        var getUsersStatus = GetUsersStatusAsync();
        var getWeatherStatus = GetWeatherStatusAsync();
        var getPingStatus = GetPingStatusAsync();
        var getSeqStatus = GetLogStatusAsync();

        try
        {
            await Task.WhenAll(getHardwareStatus, getUsersStatus, getWeatherStatus, getPingStatus, getSeqStatus);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        var line = Environment.NewLine;
        return $"Hardware:{line}{PrepareResult(getHardwareStatus)}{line}{line}" +
               $"Users:{line}{PrepareResult(getUsersStatus)}{line}{line}" +
               $"Weather:{line}{PrepareResult(getWeatherStatus)}{line}{line}" +
               $"Ping:{line}{PrepareResult(getPingStatus)}{line}{line}" +
               $"Seq:{line}{PrepareResult(getSeqStatus)}{line}{line}" +
               $"Request time: {sw.ElapsedMilliseconds} ms";
    }

    private static string PrepareResult(Task<string> task)
        => task.IsCompletedSuccessfully ? task.Result : "ERROR!";

    public async Task<string> GetHardwareStatusAsync()
    {
        var hardwareStatus = await _hardwareMonitor.GetHardwareStatusAsync();

        var line = Environment.NewLine;
        return $"CPU temp: {hardwareStatus.CpuTemperatureC:0.#} \u00b0C{line}" +
               $"CPU usage: {hardwareStatus.Cpu15MinUsagePercent:0.#} %{line}" +
               $"RAM usage: {hardwareStatus.MemoryUsagePercent:0.#} %{line}" +
               $"SSD temp: {hardwareStatus.StorageTemperatureC:0.#} \u00b0C{line}" +
               $"SSD usage: {hardwareStatus.StorageUsagePercent:0.#} %";
    }

    public async Task<string> GetUsersStatusAsync(CancellationToken ct = default)
    {
        var usersWithInactiveTime = await _userWatcher.GetUsersWithInactiveTimeAsync(_userWatcherSettings.TrackedIds, ct);

        var result = new StringBuilder();
        foreach (var (user, inactiveTime) in usersWithInactiveTime)
        {
            result.AppendLine($@"User {user.FirstName} {user.LastName} is not active for {inactiveTime:hh\:mm\:ss}");
        }

        return result.ToString().Trim();
    }

    public async Task<string> GetWeatherStatusAsync(CancellationToken ct = default)
    {
        var parseTasks = _weatherAnalyzerSettings.Devices
            .Select(d => d.Uri)
            .Select(url => _espMeteoParser.ParseAsync(url, ct));

        var espMeteos = await Task.WhenAll(parseTasks);

        var weatherStatuses = espMeteos.SelectMany(espMeteo =>
        {
            var deviceSettings = _weatherAnalyzerSettings.Devices.Single(s => s.Uri == espMeteo.Uri);
            return espMeteo.Sensors.SelectMany(sensor =>
            {
                var sensorSettings = deviceSettings.Sensors.SingleOrDefault(s => s.Name == sensor.Name);
                return sensor.Parameters.Select(parameter => $"{sensorSettings?.Alias ?? sensor.Name}.{parameter.Name}: {parameter.Value}");
            });
        });

        return string.Join(Environment.NewLine, weatherStatuses);
    }

    public async Task<string> GetPingStatusAsync()
    {
        var pingAllResponse = await _pingClient.PingAsync();

        var pingResults = pingAllResponse.PingResults
            .Select(r =>
            {
                var targetName = r.Description ?? r.Host + (r.Port.HasValue ? $":{r.Port}" : string.Empty);
                return $"{targetName}: {r.Status} {(r.Time.HasValue ? $"({r.Time.Value.Milliseconds} ms)" : string.Empty)}";
            });

        return string.Join(Environment.NewLine, pingResults);
    }

    public async Task<string> GetLogStatusAsync(CancellationToken ct = default)
    {
        var lastWeekSummary = _logAnalyzer.GetSummaryAsync(DateTimeRangeExtensions.LastWeekUtc, ct);
        var last24HoursSummary = _logAnalyzer.GetSummaryAsync(DateTimeRangeExtensions.LastDayUtc, ct);
        var last12HoursSummary = _logAnalyzer.GetSummaryAsync(DateTimeRangeExtensions.Last12HoursUtc, ct);
        var last6HoursSummary = _logAnalyzer.GetSummaryAsync(DateTimeRangeExtensions.Last6HoursUtc, ct);
        var lastHourSummary = _logAnalyzer.GetSummaryAsync(DateTimeRangeExtensions.LastHourUtc, ct);

        await Task.WhenAll(lastWeekSummary, last24HoursSummary, last12HoursSummary, last6HoursSummary, lastHourSummary)
            .ConfigureAwait(false);

        var signForWeek = _seqSettings.MaxEventsPerRequest == lastWeekSummary.Result.Count
            ? "> " : string.Empty;

        var line = Environment.NewLine;
        return $"{signForWeek}{lastWeekSummary.Result.Count} events in last week{line}" +
               $"{last24HoursSummary.Result.Count} events in 24 hours{line}" +
               $"{last12HoursSummary.Result.Count} events in 12 hours{line}" +
               $"{last6HoursSummary.Result.Count} events in 6 hours{line}" +
               $"{lastHourSummary.Result.Count} events in last hour";
    }
}
