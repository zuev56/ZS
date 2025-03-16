namespace Zs.Home.Application.Features.Hardware;

public sealed record HardwareStatus
{
    public float CpuTemperature { get; init; }
    public float CpuUsage15Min { get; init; }
    public float MemoryUsagePercent { get; init; }
    public float StorageTemperature { get; init; }
    public float StorageUsagePercent { get; init; }
}
