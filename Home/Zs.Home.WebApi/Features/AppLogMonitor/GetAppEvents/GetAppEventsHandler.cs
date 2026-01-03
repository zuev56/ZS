using MediatR;
using Zs.Home.Application.Features.Seq;

namespace Zs.Home.WebApi.Features.AppLogMonitor.GetAppEvents;

public sealed class GetAppEventsHandler : IRequestHandler<GetAppEventsRequest, GetAppEventsResponse>
{
    private readonly ILogAnalyzer _logAnalyzer;

    public GetAppEventsHandler(ILogAnalyzer logAnalyzer)
    {
        _logAnalyzer = logAnalyzer;
    }

    public async Task<GetAppEventsResponse> Handle(GetAppEventsRequest request, CancellationToken cancellationToken)
    {
        var logEntries = await _logAnalyzer.GetLogEntriesAsync(request.ToDateTimeRange(), cancellationToken);

        return new GetAppEventsResponse(logEntries);
    }
}
