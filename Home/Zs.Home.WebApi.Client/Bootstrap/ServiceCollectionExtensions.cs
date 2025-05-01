using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zs.Home.WebApi.Client.Bootstrap;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHomeClient(this IServiceCollection services, IConfiguration configuration)
    {
        var settingsSection = configuration.GetSection(HomeClientSettings.SectionName);
        services.AddOptions<HomeClientSettings>()
            .Bind(settingsSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var baseUrl = settingsSection.Get<HomeClientSettings>()!.Url;

        services.AddSingleton<IAppLogMonitorClient>(new AppLogMonitorClient(baseUrl));
        services.AddSingleton<IHardwareClient>(new HardwareClient(baseUrl));
        services.AddSingleton<IHealthCheckClient>(new HealthCheckClient(baseUrl));
        services.AddSingleton<ILanClient>(new LanClient(baseUrl));
        services.AddSingleton<IOsEventsClient>(new OsEventsClient(baseUrl));
        services.AddSingleton<IPingClient>(new PingClient(baseUrl));
        services.AddSingleton<IWeatherClient>(new WeatherClient(baseUrl));

        return services;
    }
}
