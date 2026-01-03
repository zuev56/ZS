using MediatR;
using Microsoft.Extensions.Options;

namespace Zs.Home.WebApi.Features.Weather.GetAnalysisSettings;

public sealed class GetWeatherAnalysisSettingsHandler : IRequestHandler<GetWeatherAnalysisSettingsRequest, GetWeatherAnalysisSettingsResponse>
{
    private readonly WeatherAnalyzerSettings _settings;

    public GetWeatherAnalysisSettingsHandler(IOptions<WeatherAnalyzerSettings> options)
    {
        _settings = options.Value;
    }

    public Task<GetWeatherAnalysisSettingsResponse> Handle(GetWeatherAnalysisSettingsRequest request, CancellationToken cancellationToken)
    {
        var deviceSettings = request.DeviceUri != null
            ? _settings.Devices.Where(d => d.Uri == request.DeviceUri).ToList()
            : _settings.Devices.ToList();

        return Task.FromResult(new GetWeatherAnalysisSettingsResponse(deviceSettings));
    }
}
