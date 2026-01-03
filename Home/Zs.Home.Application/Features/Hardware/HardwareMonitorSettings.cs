using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Application.Features.Hardware;

public class HardwareMonitorSettings
{
    public const string SectionName = "HardwareMonitor";

    [Required]
    public required string ShellPath { get; init; }

    [Required]
    public required Scripts Scripts { get; init; }

    [Required]
    public required Limits Limits { get; init; }
}

public sealed record Scripts
{
    [Required]
    public required string MemoryAmountGb { get; init; }
    [Required]
    public required string MemoryAvailableGb { get; init; }

    [Required]
    public required string Cpu15MinUsage { get; init; }
    [Required]
    public required string CpuTemperatureC { get; init; }

    [Required]
    public required string StorageTemperatureC { get; init; }
    [Required]
    public required string StorageAmountGb { get; init; }
    [Required]
    public required string StorageAvailableGb { get; init; }
}

public sealed record Limits
{
    [Required]
    public float CpuUsagePercent { get; init; }

    [Required]
    public float CpuTemperatureC { get; init; }

    [Required]
    public float MemoryUsagePercent { get; init; }

    [Required]
    public float StorageTemperatureC { get; init; }

    [Required]
    public float StorageUsagePercent { get; init; }
}
