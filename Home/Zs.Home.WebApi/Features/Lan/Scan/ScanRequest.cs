using MediatR;

namespace Zs.Home.WebApi.Features.Lan.Scan;

public sealed record ScanRequest : IRequest<ScanResponse>;
