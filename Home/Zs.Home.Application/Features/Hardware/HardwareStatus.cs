namespace Zs.Home.Application.Features.Hardware;

public sealed record HardwareStatus
{
    public float CpuTemperatureC { get; init; }
    public float Cpu15MinUsagePercent { get; init; }
    public float MemoryAmountGb { get; init; }
    public float MemoryAvailableGb { get; init; }
    public float MemoryUsagePercent { get; init; }
    public float StorageTemperatureC { get; init; }
    public float StorageAmountGb { get; init; }
    public float StorageAvailableGb { get; init; }
    public float StorageUsagePercent { get; init; }
}
