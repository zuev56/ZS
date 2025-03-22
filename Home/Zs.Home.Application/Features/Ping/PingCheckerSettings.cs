using System;
using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Application.Features.Ping;

public class PingCheckerSettings
{
    public const string SectionName = "PingChecker";

    public short Attempts { get; set; } = 1;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);

    [Required]
    public Target[] Targets { get; init; } = [];
}
