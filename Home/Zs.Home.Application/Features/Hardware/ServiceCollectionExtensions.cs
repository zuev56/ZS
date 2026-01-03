using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zs.Home.Application.Features.Hardware;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHardwareMonitor(this IServiceCollection services, IConfiguration configuration)
        => AddHardwareMonitor<HardwareMonitorSettings>(services, configuration);

    public static IServiceCollection AddHardwareMonitor<TSettings>(this IServiceCollection services, IConfiguration configuration)
        where TSettings : HardwareMonitorSettings
    {
        services.AddOptions<TSettings>()
            .Bind(configuration.GetSection(HardwareMonitorSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<HardwareMonitorSettings>()
            .Bind(configuration.GetSection(HardwareMonitorSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

#if DEBUG
        services.AddSingleton<IHardwareMonitor, HardwareMonitorStub>();
#else
        services.AddSingleton<IHardwareMonitor, HardwareMonitor>();
#endif

        return services;
    }
}
