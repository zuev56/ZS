using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zs.Home.WebApi.Features.OsEvents.GetEvents;
using Zs.Home.WebApi.Features.OsEvents.GetStatistics;

namespace Zs.Home.WebApi.Features.OsEvents;

[ApiController]
[Route("api/os-events")]
public class OsEventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OsEventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get events
    /// </summary>
    [HttpGet]
    [ProducesResponseType<GetEventsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSeqEvents(GetEventsRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType<GetStatisticsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(GetStatisticsRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }
}
