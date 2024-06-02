using MediatR;

namespace Zs.Home.WebApi.Features.Services.HealthCheck;

public sealed class HealthCheckHandler : IRequestHandler<HealthCheckRequest, HealthCheckResponse>
{
    public async Task<HealthCheckResponse> Handle(HealthCheckRequest request, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}
