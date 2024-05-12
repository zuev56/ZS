using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Application.Features.Hardware;

internal sealed class HardwareMonitorSettings
{
    public const string SectionName = "HardwareMonitor";

    [Required]
    public string ShellPath { get; init; } = null!;

    [Required]
    public float WarnMemoryUsage { get; init; }

    [Required]
    public float WarnCpuUsage { get; init; }

    [Required]
    public float WarnCpuTemperature { get; init; }
}
