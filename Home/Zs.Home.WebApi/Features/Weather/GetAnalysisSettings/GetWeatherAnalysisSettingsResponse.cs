namespace Zs.Home.WebApi.Features.Weather.GetAnalysisSettings;

public sealed record GetWeatherAnalysisSettingsResponse(IReadOnlyList<DeviceSettings> DeviceSettings);
