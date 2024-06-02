using MediatR;

namespace Zs.Home.WebApi.Features.Devices.GetSensor;

public sealed record GetSensorRequest : IRequest<GetSensorResponse>
{
    public short DeviceId { get; init; }
    public short SensorId { get; init; }
}
