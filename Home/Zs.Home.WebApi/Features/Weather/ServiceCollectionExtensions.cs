using Zs.Parser.EspMeteo;

namespace Zs.Home.WebApi.Features.Weather;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherAnalyzer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<WeatherAnalyzerSettings>()
            .Bind(configuration.GetSection(WeatherAnalyzerSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<EspMeteoParser>();

        services.AddSingleton<IWeatherAnalyzer, WeatherAnalyzer>();

        return services;
    }
}
