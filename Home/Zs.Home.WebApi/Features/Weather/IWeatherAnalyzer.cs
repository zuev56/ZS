namespace Zs.Home.WebApi.Features.Weather;

public interface IWeatherAnalyzer
{
    Task<IReadOnlyList<EspMeteoAnalysisResult>> AnalyseAsync(IReadOnlyList<DeviceSettings> deviceSettings, CancellationToken ct);
}
