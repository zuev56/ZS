using MediatR;
using Zs.Common.Models;
using Zs.Home.WebApi.Common;

namespace Zs.Home.WebApi.Features.AppLogMonitor.GetAppEvents;

public sealed record GetAppEventsRequest : IRequest<GetAppEventsResponse>
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }

    public GetAppEventsRequest(DateTime from, DateTime to)
        => (From, To) = (from, to);

    public GetAppEventsRequest(TimeRange timeRange)
        => (From, To) = timeRange.ToDateTimeRange();

    internal DateTimeRange ToDateTimeRange() => new(From, To);
}
