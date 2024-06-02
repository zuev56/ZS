using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zs.Home.WebApi.Features.Lan.Scan;

namespace Zs.Home.WebApi.Features.Lan;

/// <summary>
/// Local area network
/// </summary>
[ApiController]
[Route("api/lan")]
public class LanController : ControllerBase
{
    private readonly IMediator _mediator;

    public LanController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all devices from LAN
    /// </summary>
    [HttpPost("scan")]
    [ProducesResponseType<ScanResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Scan(CancellationToken cancellationToken)
    {
        var request = new ScanRequest();
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }
}
