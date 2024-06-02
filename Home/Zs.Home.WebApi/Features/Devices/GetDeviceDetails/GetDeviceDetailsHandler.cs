using MediatR;

namespace Zs.Home.WebApi.Features.Devices.GetDeviceDetails;

public sealed class GetDeviceDetailsHandler : IRequestHandler<GetDeviceDetailsRequest, GetDeviceDetailsResponse>
{
    public async Task<GetDeviceDetailsResponse> Handle(GetDeviceDetailsRequest request, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}
