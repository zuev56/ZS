using MediatR;
using Microsoft.Extensions.Options;

namespace Zs.Home.WebApi.Features.Weather.GetAnalysis;

public sealed class GetWeatherAnalysisHandler : IRequestHandler<GetWeatherAnalysisRequest, GetWeatherAnalysisResponse>
{
    private readonly IWeatherAnalyzer _weatherAnalyzer;
    private readonly WeatherAnalyzerSettings _settings;

    public GetWeatherAnalysisHandler(IWeatherAnalyzer weatherAnalyzer, IOptions<WeatherAnalyzerSettings> options)
    {
        _weatherAnalyzer = weatherAnalyzer;
        _settings = options.Value;
    }

    public async Task<GetWeatherAnalysisResponse> Handle(GetWeatherAnalysisRequest request, CancellationToken cancellationToken)
    {
        var deviceSettings = request.DeviceUri != null
            ? _settings.Devices.Where(d => d.Uri == request.DeviceUri).ToList()
            : _settings.Devices.ToList();

        var analysisResults = await _weatherAnalyzer.AnalyseAsync(deviceSettings, cancellationToken);

        return new GetWeatherAnalysisResponse(analysisResults);
    }
}
