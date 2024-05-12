using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Application.Features.Ping;

public sealed record Target
{
    [Required]
    public required string Host { get; init; } = null!;

    public int? Port { get; init; }

    public string? Description { get; init; }
}