using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zs.Home.WebApi.Common;
using Zs.Home.WebApi.Features.AppLogMonitor.GetAppEvents;
using Zs.Home.WebApi.Features.AppLogMonitor.GetAppLogSummary;

namespace Zs.Home.WebApi.Features.AppLogMonitor;

[ApiController]
[Route("api/log-monitor")]
public class AppLogMonitorController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppLogMonitorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Events
    /// </summary>
    [HttpGet("events/{from:datetime}/{to:datetime}")]
    [ProducesResponseType<GetAppEventsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventsAsync(DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAppEventsRequest(from, to), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Events
    /// </summary>
    [HttpGet("events/{timeRange}")]
    [ProducesResponseType<GetAppEventsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventsAsync(TimeRange timeRange, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAppEventsRequest(timeRange), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get the log summary
    /// </summary>
    [HttpGet("summary/{from:datetime}/{to:datetime}")]
    [ProducesResponseType<GetAppLogSummaryResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogSummaryAsync(DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAppLogSummaryRequest(from, to), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get the log summary
    /// </summary>
    [HttpGet("summary/{timeRange}")]
    [ProducesResponseType<GetAppLogSummaryResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogSummaryAsync(TimeRange timeRange, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAppLogSummaryRequest(timeRange), cancellationToken);
        return Ok(response);
    }
}
