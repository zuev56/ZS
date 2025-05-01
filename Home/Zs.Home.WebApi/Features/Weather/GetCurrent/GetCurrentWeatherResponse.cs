using Zs.Parser.EspMeteo.Models;

namespace Zs.Home.WebApi.Features.Weather.GetCurrent;

public sealed record GetCurrentWeatherResponse(IReadOnlyList<EspMeteo> Devices);
