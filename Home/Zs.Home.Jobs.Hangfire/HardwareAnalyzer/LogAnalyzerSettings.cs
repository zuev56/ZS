using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Jobs.Hangfire.HardwareAnalyzer;

public sealed class HardwareAnalyzerSettings : Application.Features.Hardware.HardwareMonitorSettings
{
    [Required]
    public required string CronExpression { get; set; }

    [Required]
    public required Limits Limits { get; init; }
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
