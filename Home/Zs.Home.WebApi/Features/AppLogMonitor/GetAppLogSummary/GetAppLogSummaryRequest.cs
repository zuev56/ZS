using MediatR;
using Zs.Home.WebApi.Common;

namespace Zs.Home.WebApi.Features.AppLogMonitor.GetAppLogSummary;

public sealed record GetAppLogSummaryRequest : IRequest<GetAppLogSummaryResponse>
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }

    public GetAppLogSummaryRequest(DateTime from, DateTime to)
        => (From, To) = (from, to);

    public GetAppLogSummaryRequest(TimeRange timeRange)
        => (From, To) = timeRange.ToDateTimeRange();
}
