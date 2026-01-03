using System;
using System.Threading;
using System.Threading.Tasks;

namespace Zs.Home.Application.Features.Hardware;

internal sealed class HardwareMonitorStub : IHardwareMonitor
{
    public Task<HardwareStatus> GetHardwareStatusAsync(CancellationToken ct = default)
    {
        var memoryAmountGb = (float)Random.Shared.Next(8, 16);
        var memoryAvailableGb = (float)Random.Shared.Next(1, 3);

        var storageAmountGb = (float)Random.Shared.Next(8, 16);
        var storageAvailableGb = (float)Random.Shared.Next(1, 3);

        return Task.FromResult(new HardwareStatus
        {
            CpuTemperatureC = Random.Shared.Next(30, 100),
            Cpu15MinUsagePercent = Random.Shared.Next(1, 100),
            MemoryAmountGb = memoryAmountGb,
            MemoryAvailableGb = memoryAvailableGb,
            MemoryUsagePercent = memoryAvailableGb / memoryAmountGb * 100,
            StorageTemperatureC = Random.Shared.Next(20, 100),
            StorageAmountGb = storageAmountGb,
            StorageAvailableGb = storageAvailableGb,
            StorageUsagePercent = storageAvailableGb / storageAmountGb * 100,
        });
    }
}
