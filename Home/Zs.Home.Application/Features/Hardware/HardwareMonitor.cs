using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Shell;

namespace Zs.Home.Application.Features.Hardware;

internal sealed class HardwareMonitor : IHardwareMonitor
{
    private readonly HardwareMonitorSettings _settings;
    private readonly ILogger<HardwareMonitor> _logger;

    public HardwareMonitor(
        IOptions<HardwareMonitorSettings> options,
        ILogger<HardwareMonitor> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<HardwareStatus> GetHardwareStatusAsync(CancellationToken ct = default)
    {
        var cpuTemperatureC = ExecuteCommandAsync<float>(_settings.Scripts.CpuTemperatureC, ct);
        var cpuUsage = ExecuteCommandAsync<float>(_settings.Scripts.Cpu15MinUsage, ct);
        var memoryAmountGb = ExecuteCommandAsync<float>(_settings.Scripts.MemoryAmountGb, ct);
        var memoryAvailableGb = ExecuteCommandAsync<float>(_settings.Scripts.MemoryAvailableGb, ct);
        var storageTemperatureC = ExecuteCommandAsync<float>(_settings.Scripts.StorageTemperatureC, ct);
        var storageAmountGb = ExecuteCommandAsync<float>(_settings.Scripts.StorageAmountGb, ct);
        var storageAvailableGb = ExecuteCommandAsync<float>(_settings.Scripts.StorageAvailableGb, ct);

        await Task.WhenAll(
                cpuTemperatureC,
                cpuUsage,
                memoryAmountGb,
                memoryAvailableGb,
                storageTemperatureC,
                storageAmountGb,
                storageAvailableGb
                )
            .ContinueWith(result =>
            {
                if (!result.IsFaulted)
                    return;

                foreach (var exception in result.Exception.InnerExceptions)
                    _logger?.LogError(exception, "Error while getting hardware status");
            }, ct);

        return new HardwareStatus
        {
            CpuTemperatureC = cpuTemperatureC.Result,
            Cpu15MinUsagePercent = cpuUsage.Result,
            MemoryAmountGb = memoryAmountGb.Result,
            MemoryAvailableGb = memoryAvailableGb.Result,
            MemoryUsagePercent = memoryAvailableGb.Result / memoryAmountGb.Result * 100,
            StorageTemperatureC = storageTemperatureC.Result,
            StorageAmountGb = storageAmountGb.Result,
            StorageAvailableGb = storageAvailableGb.Result,
            StorageUsagePercent = storageAvailableGb.Result / storageAmountGb.Result * 100,
        };
    }

    private async Task<TResult> ExecuteCommandAsync<TResult>(string command, CancellationToken ct)
    {
        var commandResult = await ShellLauncher.RunAsync(_settings.ShellPath, command, ct);

        if (!commandResult.Successful)
            throw new FaultException(commandResult.Fault!);

        if (string.IsNullOrWhiteSpace(commandResult.Value))
            throw new FaultException(Fault.Unknown.WithMessage("Empty result"));

        return (TResult)Convert.ChangeType(commandResult.Value, typeof(TResult));
    }
}
