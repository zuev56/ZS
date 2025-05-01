using MediatR;

namespace Zs.Home.WebApi.Features.HardwareStatus.GetCurrent;

public sealed record GetCurrentHardwareStatusRequest : IRequest<GetCurrentHardwareStatusResponse>;
