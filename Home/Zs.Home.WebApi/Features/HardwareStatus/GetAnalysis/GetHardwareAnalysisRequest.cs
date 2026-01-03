using MediatR;

namespace Zs.Home.WebApi.Features.HardwareStatus.GetAnalysis;

public sealed record GetHardwareAnalysisRequest : IRequest<GetHardwareAnalysisResponse>;
