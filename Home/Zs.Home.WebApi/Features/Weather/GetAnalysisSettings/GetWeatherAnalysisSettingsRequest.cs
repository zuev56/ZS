using MediatR;

namespace Zs.Home.WebApi.Features.Weather.GetAnalysisSettings;

public sealed record GetWeatherAnalysisSettingsRequest : IRequest<GetWeatherAnalysisSettingsResponse>
{
    public GetWeatherAnalysisSettingsRequest(string? deviceUri = null)
    {
        DeviceUri = deviceUri;
    }

    public string? DeviceUri { get; }
}
