using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Zs.Home.Application.Features.Hardware;

internal abstract class HardwareMonitor
{
    protected readonly HardwareMonitorSettings Options;
    protected readonly ILogger<HardwareMonitor> Logger;

    public string CliPath => Options.ShellPath;

    protected HardwareMonitor(
        IOptions<HardwareMonitorSettings> options,
        ILogger<HardwareMonitor> logger)
    {
        Options = options.Value;
        Logger = logger;
    }

    protected abstract Task<float> GetCpuTemperatureAsync();
    protected abstract Task<float> Get15MinAvgCpuUsageAsync();
    protected abstract Task<float> GetMemoryUsagePercentAsync();
    protected abstract Task<float> GetStorageTemperatureAsync();
    protected abstract Task<float> GetStorageUsagePercentAsync();

    public async Task<HardwareStatus> GetHardwareStatusAsync()
    {
        var cpuTemperature = GetCpuTemperatureAsync();
        var cpuUsage = Get15MinAvgCpuUsageAsync();
        var memoryUsage = GetMemoryUsagePercentAsync();
        var storageTemperature = GetStorageTemperatureAsync();
        var storageUsage = GetStorageUsagePercentAsync();

        await Task.WhenAll(
                cpuTemperature,
                cpuUsage,
                memoryUsage,
                storageTemperature,
                storageUsage)
            .ContinueWith(result =>
            {
                if (!result.IsFaulted)
                    return;

                foreach (var exception in result.Exception.InnerExceptions)
                    Logger?.LogError(exception, "Error while getting hardware status");
            });

        return new HardwareStatus
        {
            CpuTemperature = cpuTemperature.Result,
            CpuUsage15Min = cpuUsage.Result,
            MemoryUsagePercent = memoryUsage.Result,
            StorageTemperature = storageTemperature.Result,
            StorageUsagePercent = storageUsage.Result,
        };
    }
}
