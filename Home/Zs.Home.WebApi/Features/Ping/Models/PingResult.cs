using System.Net.NetworkInformation;

namespace Zs.Home.WebApi.Features.Ping.Models;

public sealed record PingResult
{
    public required string Host { get; init; } = null!;
    public short? Port { get; init; }
    public string? Description { get; init; }
    public required IPStatus Status { get; init; }
    public TimeSpan? Time { get; init; }
}
