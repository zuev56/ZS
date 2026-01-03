using System.ComponentModel.DataAnnotations;

namespace Zs.Home.WebApi.Features.Ping.Models;

public sealed class PingCheckerSettings
{
    public const string SectionName = "PingChecker";

    public short Attempts { get; set; } = 1;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);

    [Required]
    public Target[] Targets { get; init; } = [];
}
