using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Home.Jobs.Hangfire.Extensions;
using Zs.Home.Jobs.Hangfire.Notification;
using Zs.Home.WebApi;

namespace Zs.Home.Jobs.Hangfire.HardwareAnalyzer;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class HardwareAnalyzerJob
{
    private readonly IHardwareClient _hardwareClient;
    private readonly Notifier _notifier;
    private readonly ILogger<HardwareAnalyzerJob> _logger;

    public HardwareAnalyzerJob(
        IHardwareClient hardwareClient,
        Notifier notifier,
        ILogger<HardwareAnalyzerJob> logger)
    {
        _hardwareClient = hardwareClient;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogJobStart();

        var statusResponse = await _hardwareClient.GetCurrentHardwareStatusAsync(ct);
        var limitsResponse = await _hardwareClient.GetHardwareStatusLimitsAsync(ct);

        var notification = CreateNotification(statusResponse.HardwareStatus, limitsResponse.Limits);

        await _notifier.SendNotificationAsync(notification, ct);

        _logger.LogJobFinish(sw.Elapsed);
    }

    private string CreateNotification(HardwareStatus hardwareStatus, Limits limits)
    {
        var notification = new StringBuilder();
        if (hardwareStatus.CpuTemperatureC > limits.CpuTemperatureC)
            notification.AppendLine($"CPU temperature: {hardwareStatus.CpuTemperatureC:0.#} \u00b0C");
        if (hardwareStatus.Cpu15MinUsagePercent > limits.CpuUsagePercent)
            notification.AppendLine($"CPU usage: {hardwareStatus.Cpu15MinUsagePercent:0.#} %");
        if (hardwareStatus.MemoryUsagePercent > limits.MemoryUsagePercent)
            notification.AppendLine($"RAM usage: {hardwareStatus.MemoryUsagePercent:0.#} %");
        if (hardwareStatus.StorageTemperatureC > limits.StorageTemperatureC)
            notification.AppendLine($"SSD temperature: {hardwareStatus.StorageTemperatureC:0.#} \u00b0C");
        if (hardwareStatus.StorageUsagePercent > limits.StorageUsagePercent)
            notification.AppendLine($"Storage usage: {hardwareStatus.StorageUsagePercent:0.#} %");

        return notification.ToString();
    }
}
