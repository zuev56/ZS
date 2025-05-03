using MediatR;

namespace Zs.Home.WebApi.Features.Ping.Models;

public sealed record PingRequest : IRequest<PingResponse>
{
    public Target? Target { get; init; }
}
