using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zs.Home.WebApi.Features.HardwareStatus.GetCurrent;
using Zs.Home.WebApi.Features.HardwareStatus.GetStatusLimits;

namespace Zs.Home.WebApi.Features.HardwareStatus;

[ApiController]
[Route("api/hardware")]
public class HardwareController : ControllerBase
{
    private readonly IMediator _mediator;

    public HardwareController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get the current hardware status
    /// </summary>
    [HttpGet("current-status")]
    [ProducesResponseType<GetCurrentHardwareStatusResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentHardwareStatusAsync(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetCurrentHardwareStatusRequest(), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get the hardware status limits
    /// </summary>
    [HttpGet("limits")]
    [ProducesResponseType<GetHardwareStatusLimitsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHardwareStatusLimitsAsync(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetHardwareStatusLimitsRequest(), cancellationToken);
        return Ok(response);
    }

    // /// <summary>
    // /// Get the current hardware status analysis
    // /// </summary>
    // [HttpGet("analysis")]
    // [ProducesResponseType<GetHardwareAnalysisResponse>(StatusCodes.Status200OK)]
    // public async Task<IActionResult> GetAnalysisAsync(CancellationToken cancellationToken)
    // {
    //     var response = await _mediator.Send(new GetHardwareAnalysisRequest(), cancellationToken);
    //     return Ok(response);
    // }
}
