using MediatR;

namespace Zs.Home.WebApi.Features.Weather.GetAnalysis;

public sealed record GetWeatherAnalysisRequest : IRequest<GetWeatherAnalysisResponse>
{
    public GetWeatherAnalysisRequest(string? deviceUri = null)
    {
        DeviceUri = deviceUri;
    }

    public string? DeviceUri { get; }
}
