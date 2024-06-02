using MediatR;

namespace Zs.Home.WebApi.Features.OsEvents.GetStatistics;

public sealed class GetStatisticsHandler : IRequestHandler<GetStatisticsRequest, GetStatisticsResponse>
{
    public async Task<GetStatisticsResponse> Handle(GetStatisticsRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
