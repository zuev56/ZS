using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zs.Home.WebApi.Features.Weather.GetAnalysis;
using Zs.Home.WebApi.Features.Weather.GetAnalysisSettings;
using Zs.Home.WebApi.Features.Weather.GetCurrent;

namespace Zs.Home.WebApi.Features.Weather;

[ApiController]
[Route("api/weather")]
public class WeatherController : ControllerBase
{
    private readonly IMediator _mediator;

    public WeatherController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get the current weather.
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType<GetCurrentWeatherResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrent(string? deviceUri, CancellationToken cancellationToken)
    {
        if (!IsEmptyOrCorrectUri(deviceUri))
            return BadRequest();

        var response = await _mediator.Send(new GetCurrentWeatherRequest(deviceUri), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get the current weather analysis from the all devices.
    /// </summary>
    [HttpGet("full-analysis")]
    [ProducesResponseType<GetWeatherAnalysisResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFullAnalysis(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetWeatherAnalysisRequest(), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get the current weather analysis.
    /// </summary>
    [HttpGet("analysis")]
    [ProducesResponseType<GetWeatherAnalysisResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalysis(string? deviceUri, CancellationToken cancellationToken)
    {
        if (!IsEmptyOrCorrectUri(deviceUri))
            return BadRequest();

        var response = await _mediator.Send(new GetWeatherAnalysisRequest(deviceUri), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get the weather analysis settings
    /// </summary>
    [HttpGet("settings")]
    [ProducesResponseType<GetWeatherAnalysisSettingsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings(string? deviceUri, CancellationToken cancellationToken)
    {
        if (!IsEmptyOrCorrectUri(deviceUri))
            return BadRequest();

        var response = await _mediator.Send(new GetWeatherAnalysisSettingsRequest(deviceUri), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get the weather analysis settings
    /// </summary>
    [HttpGet("all-settings")]
    [ProducesResponseType<GetWeatherAnalysisSettingsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSettings(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetWeatherAnalysisSettingsRequest(), cancellationToken);
        return Ok(response);
    }

    private static bool IsEmptyOrCorrectUri(string? uri)
        => string.IsNullOrWhiteSpace(uri) || Uri.IsWellFormedUriString(uri, UriKind.Absolute);
}
