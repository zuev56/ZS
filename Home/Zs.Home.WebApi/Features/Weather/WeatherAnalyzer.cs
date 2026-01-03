using Zs.Parser.EspMeteo;
using Zs.Parser.EspMeteo.Models;

namespace Zs.Home.WebApi.Features.Weather;

internal sealed class WeatherAnalyzer : IWeatherAnalyzer
{
    private readonly EspMeteoParser _espMeteoParser;
    private readonly ILogger<WeatherAnalyzer>? _logger;

    public WeatherAnalyzer(
        EspMeteoParser espMeteoParser,
        ILogger<WeatherAnalyzer>? logger)
    {
        _espMeteoParser = espMeteoParser;
        _logger = logger;
    }

    public async Task<IReadOnlyList<EspMeteoAnalysisResult>> AnalyseAsync(IReadOnlyList<DeviceSettings> deviceSettings, CancellationToken ct)
    {
        var espMeteos = await GetEspMeteoInfosAsync(deviceSettings, CancellationToken.None);
        var analysisResults = new List<EspMeteoAnalysisResult>();

        foreach (var espMeteo in espMeteos)
        {
            var settings = deviceSettings.Single(s => s.Uri == espMeteo.Uri);
            analysisResults.Add(new EspMeteoAnalysisResult(espMeteo, settings));
        }

        return analysisResults;
    }

    private async Task<EspMeteo[]> GetEspMeteoInfosAsync(IReadOnlyList<DeviceSettings> deviceSettings, CancellationToken ct)
    {
        var parseTasks = deviceSettings
            .Select(static d => d.Uri)
            .Select(url => _espMeteoParser.ParseAsync(url, ct));

        return await Task.WhenAll(parseTasks);
    }
}
