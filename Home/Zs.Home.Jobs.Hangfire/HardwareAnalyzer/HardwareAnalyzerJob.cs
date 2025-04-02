using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Home.Application.Features.Hardware;
using Zs.Home.Jobs.Hangfire.Extensions;
using Zs.Home.Jobs.Hangfire.Notification;

namespace Zs.Home.Jobs.Hangfire.HardwareAnalyzer;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class HardwareAnalyzerJob
{
    private readonly IHardwareMonitor _hardwareMonitor;
    private readonly Limits _limits;
    private readonly Notifier _notifier;
    private readonly ILogger<HardwareAnalyzerJob> _logger;

    public HardwareAnalyzerJob(
        IHardwareMonitor hardwareMonitor,
        IOptions<HardwareAnalyzerSettings> settings,
        Notifier notifier,
        ILogger<HardwareAnalyzerJob> logger)
    {
        _hardwareMonitor = hardwareMonitor;
        _limits = settings.Value.Limits;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogJobStart();

        var hardwareStatus = await _hardwareMonitor.GetHardwareStatusAsync(ct);

        var notification = CreateNotification(hardwareStatus);

        await _notifier.SendNotificationAsync(notification, ct);

        _logger.LogJobFinish(sw.Elapsed);
    }

    private string CreateNotification(HardwareStatus hardwareStatus)
    {
        var notification = new StringBuilder();
        if (hardwareStatus.CpuTemperatureC > _limits.CpuTemperatureC)
            notification.AppendLine($"CPU temperature: {hardwareStatus.CpuTemperatureC:0.#} \u00b0C");
        if (hardwareStatus.Cpu15MinUsagePercent > _limits.CpuUsagePercent)
            notification.AppendLine($"CPU usage: {hardwareStatus.Cpu15MinUsagePercent:0.#} %");
        if (hardwareStatus.MemoryUsagePercent > _limits.MemoryUsagePercent)
            notification.AppendLine($"RAM usage: {hardwareStatus.MemoryUsagePercent:0.#} %");
        if (hardwareStatus.StorageTemperatureC > _limits.StorageTemperatureC)
            notification.AppendLine($"SSD temperature: {hardwareStatus.StorageTemperatureC:0.#} \u00b0C");
        if (hardwareStatus.StorageUsagePercent > _limits.StorageUsagePercent)
            notification.AppendLine($"Storage usage: {hardwareStatus.StorageUsagePercent:0.#} %");

        return notification.ToString();
    }
}
