using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zs.Home.WebApi.Features.Ping.Models;

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
    /// Ping all hosts from API settings
    /// </summary>
    [HttpGet]
    [ProducesResponseType<PingResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> PingAllAsync(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new PingRequest(), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Ping
    /// </summary>
    [HttpGet("{host}")]
    [ProducesResponseType<PingResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> PingAsync(string host, short? port, CancellationToken cancellationToken)
    {
        var request = new PingRequest { Target = new Target { Host = host, Port = port } };
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }
}
