using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zs.Home.WebApi.Features.Services.GetServiceDetails;
using Zs.Home.WebApi.Features.Services.GetServices;
using Zs.Home.WebApi.Features.Services.HealthCheck;

namespace Zs.Home.WebApi.Features.Services;

[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all services
    /// </summary>
    [HttpGet]
    [ProducesResponseType<GetServicesResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(GetServicesRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get service details
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<GetServiceDetailsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDetails(int serviceId, CancellationToken cancellationToken)
    {
        var request = new GetServiceDetailsRequest { ServiceId = serviceId };
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Check a service current state
    /// </summary>
    [HttpGet("{id:int}/health-check")]
    [ProducesResponseType<HealthCheckResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> HealthCheck(int serviceId, CancellationToken cancellationToken)
    {
        var request = new HealthCheckRequest { ServiceId = serviceId };
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }
}
