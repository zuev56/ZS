using MediatR;

namespace Zs.Home.WebApi.Features.Services.GetServices;

public sealed record GetServicesRequest : IRequest<GetServicesResponse>
{
    public bool MyOnly { get; set; }
}
