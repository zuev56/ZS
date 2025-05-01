using MediatR;

namespace Zs.Home.WebApi.Features.HardwareStatus.GetStatusLimits;

public sealed record GetHardwareStatusLimitsRequest : IRequest<GetHardwareStatusLimitsResponse>;
