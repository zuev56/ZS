using MediatR;

namespace Zs.Home.WebApi.Features.SeqEvents.GetStatistics;

public sealed class GetStatisticsHandler : IRequestHandler<GetStatisticsRequest, GetStatisticsResponse>
{
    public async Task<GetStatisticsResponse> Handle(GetStatisticsRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
