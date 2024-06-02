using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zs.Home.WebApi.Features.Devices.GetDeviceDetails;
using Zs.Home.WebApi.Features.Devices.GetDevices;
using Zs.Home.WebApi.Features.Devices.GetSensor;

namespace Zs.Home.WebApi.Features.Devices;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DevicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all devices
    /// </summary>
    [HttpGet]
    [ProducesResponseType<GetDevicesResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDevices(CancellationToken cancellationToken)
    {
        var request = new GetDevicesRequest();
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get detailed device info
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<GetDeviceDetailsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeviceDetails(short id, CancellationToken cancellationToken)
    {
        var request = new GetDeviceDetailsRequest { DeviceId = id };
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get sensor info
    /// </summary>
    [HttpGet("{id:int}/sensor/{type:int}")]
    [ProducesResponseType<GetSensorResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSensor(short deviceId, short sensorId, CancellationToken cancellationToken)
    {
        var request = new GetSensorRequest { DeviceId = deviceId, SensorId = sensorId };
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }
}
