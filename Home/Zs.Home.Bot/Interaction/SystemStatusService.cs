using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Zs.Home.Application.Features.Hardware;
using Zs.Home.Application.Features.Ping;
using Zs.Home.Application.Features.Seq;
using Zs.Home.Application.Features.VkUsers;
using Zs.Home.Application.Features.Weather;

namespace Zs.Home.Bot.Interaction;

internal sealed class SystemStatusService
{
    private readonly IHardwareMonitor _hardwareMonitor;
    private readonly IUserWatcher _userWatcher;
    private readonly IWeatherAnalyzer _weatherAnalyzer;
    private readonly IPingChecker _pingChecker;
    private readonly ISeqEventsInformer _seqEventsInformer;

    public SystemStatusService(
        IHardwareMonitor hardwareMonitor,
        IUserWatcher userWatcher,
        IWeatherAnalyzer weatherAnalyzer,
        IPingChecker pingChecker,
        ISeqEventsInformer seqEventsInformer)
    {
        _hardwareMonitor = hardwareMonitor;
        _userWatcher = userWatcher;
        _weatherAnalyzer = weatherAnalyzer;
        _pingChecker = pingChecker;
        _seqEventsInformer = seqEventsInformer;
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

    public Task<string> GetHardwareStatusAsync() => _hardwareMonitor.GetCurrentStateAsync();

    public Task<string> GetUsersStatusAsync() => _userWatcher.GetCurrentStateAsync();

    public Task<string> GetWeatherStatusAsync() => _weatherAnalyzer.GetCurrentStateAsync();

    public Task<string> GetPingStatusAsync() => _pingChecker.GetCurrentStateAsync();

    public Task<string> GetSeqStatusAsync() => _seqEventsInformer.GetCurrentStateAsync();
}
