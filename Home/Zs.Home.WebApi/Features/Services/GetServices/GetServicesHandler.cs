using MediatR;

namespace Zs.Home.WebApi.Features.Services.GetServices;

public sealed class GetServicesHandler : IRequestHandler<GetServicesRequest, GetServicesResponse>
{
    public async Task<GetServicesResponse> Handle(GetServicesRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
