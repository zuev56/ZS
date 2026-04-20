using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Zs.Home.AIAgent.Worker.Plugins.Weather;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherPlugin(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<WeatherPluginSettings>()
            .Bind(configuration.GetSection(WeatherPluginSettings.SectionName))
            .ValidateOnStart();

        services.AddHttpClient<WeatherPlugin>();

        services.AddSingleton<WeatherPlugin>();

        var serviceProvider = services.BuildServiceProvider();

        var weatherPlugin = serviceProvider.GetRequiredService<WeatherPlugin>();

        var kernel = serviceProvider.GetRequiredService<Kernel>();
        kernel.Plugins.AddFromObject(weatherPlugin, WeatherPluginSettings.PluginName);

        return services;
    }
}
