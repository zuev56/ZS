using MediatR;

namespace Zs.Home.WebApi.Features.Lan.Scan;

public sealed class ScanHandler : IRequestHandler<ScanRequest, ScanResponse>
{
    public async Task<ScanResponse> Handle(ScanRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
