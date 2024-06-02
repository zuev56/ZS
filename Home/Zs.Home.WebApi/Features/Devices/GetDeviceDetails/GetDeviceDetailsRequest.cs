using MediatR;

namespace Zs.Home.WebApi.Features.Devices.GetDeviceDetails;

public sealed record GetDeviceDetailsRequest : IRequest<GetDeviceDetailsResponse>
{
    public short DeviceId { get; init; }
}
