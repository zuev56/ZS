using System;
using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Bot.Features.Ping;

public sealed class PingCheckerSettings
{
    public const string SectionName = "PingChecker";

    [Required]
    public Target[] Targets { get; init; } = Array.Empty<Target>();
}