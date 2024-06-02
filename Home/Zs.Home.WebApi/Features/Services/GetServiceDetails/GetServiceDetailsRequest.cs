using MediatR;

namespace Zs.Home.WebApi.Features.Services.GetServiceDetails;

public sealed record GetServiceDetailsRequest : IRequest<GetServiceDetailsResponse>
{
    public required int ServiceId { get; init; }
}
