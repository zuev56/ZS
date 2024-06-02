using MediatR;

namespace Zs.Home.WebApi.Features.Ping.Ping;

public sealed record PingRequest : IRequest<PingResponse>
{
    public required string Address { get; init; }
    public string? Port { get; init; }
}
