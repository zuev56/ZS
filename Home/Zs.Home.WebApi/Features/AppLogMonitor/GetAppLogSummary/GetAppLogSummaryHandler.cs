using MediatR;
using Zs.Common.Models;
using Zs.Home.Application.Features.Seq;

namespace Zs.Home.WebApi.Features.AppLogMonitor.GetAppLogSummary;

public sealed class GetAppLogSummaryHandler : IRequestHandler<GetAppLogSummaryRequest, GetAppLogSummaryResponse>
{
    private readonly ILogAnalyzer _logAnalyzer;

    public GetAppLogSummaryHandler(ILogAnalyzer logAnalyzer)
    {
        _logAnalyzer = logAnalyzer;
    }

    public async Task<GetAppLogSummaryResponse> Handle(GetAppLogSummaryRequest request, CancellationToken cancellationToken)
    {
        var dateTimeRange = new DateTimeRange(request.From, request.To);
        var logSummary = await _logAnalyzer.GetSummaryAsync(dateTimeRange, cancellationToken);

        return new GetAppLogSummaryResponse(logSummary);
    }
}
