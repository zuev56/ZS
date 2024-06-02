using MediatR;
using Zs.Home.WebApi.Common;

namespace Zs.Home.WebApi.Features.SeqEvents.GetEvents;

public sealed record GetEventsRequest : IRequest<GetEventsResponse>
{
    public TimeRange TimeRange { get; set; }
}
