using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zs.Home.WebApi.Features.Ping.Ping;

namespace Zs.Home.WebApi.Features.Ping;

[ApiController]
[Route("api/ping")]
public class PingController : ControllerBase
{
    private readonly IMediator _mediator;

    public PingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Ping
    /// </summary>
    [HttpGet]
    [ProducesResponseType<PingResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSeqEvents(PingRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }
}
