using MediatR;

namespace Zs.Home.WebApi.Features.Devices.GetSensor;

public sealed class GetSensorHandler : IRequestHandler<GetSensorRequest, GetSensorResponse>
{
    public async Task<GetSensorResponse> Handle(GetSensorRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
