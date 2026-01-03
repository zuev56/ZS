using MediatR;
using Microsoft.Extensions.Options;
using Zs.Parser.EspMeteo;

namespace Zs.Home.WebApi.Features.Weather.GetCurrent;

public sealed class GetCurrentWeatherHandler : IRequestHandler<GetCurrentWeatherRequest, GetCurrentWeatherResponse>
{
    private readonly EspMeteoParser _espMeteoParser;
    private readonly WeatherAnalyzerSettings _settings;

    public GetCurrentWeatherHandler(EspMeteoParser espMeteoParser, IOptions<WeatherAnalyzerSettings> options)
    {
        _espMeteoParser = espMeteoParser;
        _settings = options.Value;
    }

    public async Task<GetCurrentWeatherResponse> Handle(GetCurrentWeatherRequest request, CancellationToken cancellationToken)
    {
        var urls = request.DeviceUri != null
            ? [request.DeviceUri]
            : _settings.Devices.Select(d => d.Uri);

        var espMeteos = await Task
            .WhenAll(urls.Select(url => _espMeteoParser.ParseAsync(url, cancellationToken)));

        return new GetCurrentWeatherResponse(espMeteos);
    }
}
