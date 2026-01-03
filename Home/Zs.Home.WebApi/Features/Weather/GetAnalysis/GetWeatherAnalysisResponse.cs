namespace Zs.Home.WebApi.Features.Weather.GetAnalysis;

public sealed record GetWeatherAnalysisResponse(IReadOnlyList<EspMeteoAnalysisResult> EspMeteoAnalysisResults)
{
    public IReadOnlyDictionary<int, string> DeviationTypeCodeToNameMap => EspMeteoAnalysisResults
            .SelectMany(r => r.Deviations.Select(d => d.Type))
            .Distinct()
            .ToDictionary(d => (int)d, d => d.ToString());
}
