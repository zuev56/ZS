using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Zs.Home.Application.Features.Hardware;
using Zs.Home.Application.Features.Ping;
using Zs.Home.Application.Features.Seq;
using Zs.Home.Application.Features.VkUsers;
using Zs.Home.Application.Features.Weather;
using Zs.Parser.EspMeteo;

namespace Zs.Home.Bot.Interaction;

internal sealed class SystemStatusService
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(1);

    private readonly IHardwareMonitor _hardwareMonitor;
    private readonly IUserWatcher _userWatcher;
    private readonly EspMeteoParser _espMeteoParser;
    private readonly IPingChecker _pingChecker;
    private readonly ISeqEventsInformer _seqEventsInformer;
    private readonly UserWatcherSettings _userWatcherSettings;
    private readonly WeatherAnalyzerSettings _weatherAnalyzerSettings;

    public SystemStatusService(
        IHardwareMonitor hardwareMonitor,
        IUserWatcher userWatcher,
        EspMeteoParser espMeteoParser,
        IPingChecker pingChecker,
        ISeqEventsInformer seqEventsInformer,
        IOptions<UserWatcherSettings> userWatcherSettings,
        IOptions<WeatherAnalyzerSettings> weatherAnalyzerSettings)
    {
        _hardwareMonitor = hardwareMonitor;
        _userWatcher = userWatcher;
        _pingChecker = pingChecker;
        _seqEventsInformer = seqEventsInformer;
        _espMeteoParser = espMeteoParser;
        _userWatcherSettings = userWatcherSettings.Value;
        _weatherAnalyzerSettings = weatherAnalyzerSettings.Value;
    }

    public async Task<string> GetFullStatus()
    {
        var sw = Stopwatch.StartNew();
        var getHardwareStatus = GetHardwareStatusAsync();
        var getUsersStatus = GetUsersStatusAsync();
        var getWeatherStatus = GetWeatherStatusAsync();
        var getPingStatus = GetPingStatusAsync();
        var getSeqStatus = GetSeqStatusAsync();

        await Task.WhenAll(getHardwareStatus, getUsersStatus, getWeatherStatus, getPingStatus, getSeqStatus);

        var nl = Environment.NewLine;
        return $"Hardware:{nl}{getHardwareStatus.Result}{nl}{nl}" +
               $"Users:{nl}{getUsersStatus.Result}{nl}{nl}" +
               $"Weather:{nl}{getWeatherStatus.Result}{nl}{nl}" +
               $"Ping:{nl}{getPingStatus.Result}{nl}{nl}" +
               $"Seq:{nl}{getSeqStatus.Result}{nl}{nl}" +
               $"Request time: {sw.ElapsedMilliseconds} ms";
    }

    public Task<string> GetHardwareStatusAsync() => _hardwareMonitor.GetCurrentStateAsync(_defaultTimeout);

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
            .Select(static d => d.Uri)
            .Select(url => _espMeteoParser.ParseAsync(url, ct));

        var espMeteos =  await Task.WhenAll(parseTasks);

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

    public Task<string> GetPingStatusAsync() => _pingChecker.GetCurrentStateAsync(_defaultTimeout);

    public Task<string> GetSeqStatusAsync() => _seqEventsInformer.GetCurrentStateAsync(_defaultTimeout);
}
