using MediatR;

namespace Zs.Home.WebApi.Features.Devices.GetDevices;

public sealed record GetDevicesRequest : IRequest<GetDevicesResponse>;
