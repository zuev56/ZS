using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Zs.Home.Application.Features.VkUsers;
using Zs.Home.WebApi;
using static System.DateTimeOffset;

namespace Zs.Home.Bot.Interaction;

internal sealed class SystemStatusService
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(1);

    private readonly IHardwareClient _hardwareClient;
    private readonly IUserWatcher _userWatcher;
    private readonly IPingClient _pingClient;
    private readonly IWeatherClient _weatherClient;
    private readonly IAppLogMonitorClient _appLogMonitorClient;
    private readonly UserWatcherSettings _userWatcherSettings;

    public SystemStatusService(
        IHardwareClient hardwareClient,
        IUserWatcher userWatcher,
        IPingClient pingClient,
        IWeatherClient weatherClient,
        IAppLogMonitorClient appLogMonitorClient,
        IOptions<UserWatcherSettings> userWatcherSettings)
    {
        _hardwareClient = hardwareClient;
        _userWatcher = userWatcher;
        _pingClient = pingClient;
        _weatherClient = weatherClient;
        _appLogMonitorClient = appLogMonitorClient;
        _userWatcherSettings = userWatcherSettings.Value;
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
        var hardwareStatusResponse = await _hardwareClient.GetCurrentHardwareStatusAsync();
        var status = hardwareStatusResponse.HardwareStatus;

        var line = Environment.NewLine;
        return $"CPU: {status.Cpu15MinUsagePercent:0} %, {status.CpuTemperatureC:0.#} \u00b0C{line}" +
               $"RAM: {status.MemoryUsagePercent:0} %{line}" +
               $"SSD: {status.StorageUsagePercent:0} %, {status.StorageTemperatureC:0.#} \u00b0C";
    }

    public async Task<string> GetUsersStatusAsync(CancellationToken ct = default)
    {
        var usersWithInactiveTime = await _userWatcher.GetUsersWithInactiveTimeAsync(_userWatcherSettings.TrackedIds, ct);

        var result = new StringBuilder();
        foreach (var (user, inactiveTime) in usersWithInactiveTime)
        {
            result.AppendLine($@"{user.FirstName} {user.LastName} is not active for {inactiveTime:hh\:mm\:ss}");
        }

        return result.ToString().Trim();
    }

    public async Task<string> GetWeatherStatusAsync(CancellationToken ct = default)
    {
        var weatherAnalysisResponse = await _weatherClient.GetCurrentAsync(ct);
        var settingsResponse = await _weatherClient.GetAllSettingsAsync(ct);

        var weatherStatuses = weatherAnalysisResponse.Devices.SelectMany(espMeteo =>
        {
            var deviceSettings = settingsResponse.DeviceSettings.Single(s => s.Uri.ToString() == espMeteo.Uri);
            return espMeteo.Sensors.SelectMany(sensor =>
            {
                var sensorSettings = deviceSettings.Sensors.SingleOrDefault(s => s.Name == sensor.Name);
                return sensor.Parameters.Select(parameter => $"{sensorSettings?.Alias ?? sensor.Name}.{parameter.Name}: {parameter.Value} {parameter.Unit}");
            });
        });

        return string.Join(Environment.NewLine, weatherStatuses);
    }

    public async Task<string> GetPingStatusAsync()
    {
        var pingAllResponse = await _pingClient.PingAllAsync();

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
        var lastWeekSummary = _appLogMonitorClient.GetLogSummaryAsync(Now.AddDays(-7), Now, ct);
        var last24HoursSummary = _appLogMonitorClient.GetLogSummaryAsync(Now.AddHours(-24), Now, ct);
        var last12HoursSummary = _appLogMonitorClient.GetLogSummaryAsync(Now.AddHours(-12), Now, ct);
        var last6HoursSummary = _appLogMonitorClient.GetLogSummaryAsync(Now.AddHours(-6), Now, ct);
        var lastHourSummary = _appLogMonitorClient.GetLogSummaryAsync(Now.AddHours(-1), Now, ct);

        await Task.WhenAll(lastWeekSummary, last24HoursSummary, last12HoursSummary, last6HoursSummary, lastHourSummary)
            .ConfigureAwait(false);

        return $"{lastWeekSummary.Result.LogSummary.Count}/week, " +
               $"{last24HoursSummary.Result.LogSummary.Count}/24h, " +
               $"{last12HoursSummary.Result.LogSummary.Count}/12h, " +
               $"{last6HoursSummary.Result.LogSummary.Count}/6h, " +
               $"{lastHourSummary.Result.LogSummary.Count}/1h{Environment.NewLine}";
    }
}
