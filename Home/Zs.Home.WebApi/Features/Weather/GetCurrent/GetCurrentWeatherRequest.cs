using MediatR;

namespace Zs.Home.WebApi.Features.Weather.GetCurrent;

public sealed record GetCurrentWeatherRequest : IRequest<GetCurrentWeatherResponse>
{
    public GetCurrentWeatherRequest(string? deviceUri)
    {
        DeviceUri = deviceUri;
    }

    public string? DeviceUri { get; }
}
