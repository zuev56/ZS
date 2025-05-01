using MediatR;
using Zs.Home.Application.Features.Ping;

namespace Zs.Home.WebApi.Features.Ping;

public sealed record PingRequest : IRequest<PingResponse>
{
    public Target? Target { get; init; }
}
