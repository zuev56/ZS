using MediatR;

namespace Zs.Home.WebApi.Features.Devices.GetDevices;

public sealed class GetDevicesHandler : IRequestHandler<GetDevicesRequest, GetDevicesResponse>
{
    public async Task<GetDevicesResponse> Handle(GetDevicesRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
