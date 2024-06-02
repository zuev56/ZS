using MediatR;

namespace Zs.Home.WebApi.Features.Ping.Ping;

public sealed class PingHandler : IRequestHandler<PingRequest, PingResponse>
{
    public async Task<PingResponse> Handle(PingRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
