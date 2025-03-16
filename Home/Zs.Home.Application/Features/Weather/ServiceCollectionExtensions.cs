using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zs.Parser.EspMeteo;

namespace Zs.Home.Application.Features.Weather;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherAnalyzer(this IServiceCollection services, IConfiguration configuration)
        => AddWeatherAnalyzer<WeatherAnalyzerSettings>(services, configuration);

    public static IServiceCollection AddWeatherAnalyzer<TSettings>(this IServiceCollection services, IConfiguration configuration)
        where TSettings : WeatherAnalyzerSettings
    {
        services.AddOptions<TSettings>()
            .Bind(configuration.GetSection(WeatherAnalyzerSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<EspMeteoParser>();

        services.AddSingleton<IWeatherAnalyzer, WeatherAnalyzer>();

        return services;
    }
}
