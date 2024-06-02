using MediatR;

namespace Zs.Home.WebApi.Features.OsEvents.GetEvents;

public sealed class GetEventsHandler : IRequestHandler<GetEventsRequest, GetEventsResponse>
{
    public async Task<GetEventsResponse> Handle(GetEventsRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
