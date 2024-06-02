using MediatR;

namespace Zs.Home.WebApi.Features.Services.GetServiceDetails;

public sealed class GetServiceDetailsHandler : IRequestHandler<GetServiceDetailsRequest, GetServiceDetailsResponse>
{
    public async Task<GetServiceDetailsResponse> Handle(GetServiceDetailsRequest request, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}
