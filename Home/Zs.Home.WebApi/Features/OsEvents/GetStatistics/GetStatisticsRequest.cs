using MediatR;
using Zs.Home.WebApi.Common;

namespace Zs.Home.WebApi.Features.OsEvents.GetStatistics;

public sealed record GetStatisticsRequest : IRequest<GetStatisticsResponse>
{
    public TimeRange TimeRange { get; set; }
}
