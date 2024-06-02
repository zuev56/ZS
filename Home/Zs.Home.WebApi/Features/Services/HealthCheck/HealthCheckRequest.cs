using MediatR;

namespace Zs.Home.WebApi.Features.Services.HealthCheck;

public sealed record HealthCheckRequest : IRequest<HealthCheckResponse>
{
    public required int ServiceId { get; init; }
}
