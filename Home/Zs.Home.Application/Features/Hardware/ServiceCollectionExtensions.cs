using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zs.Home.Application.Features.Hardware;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLinuxHardwareMonitor(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<HardwareMonitorSettings>()
            .Bind(configuration.GetSection(HardwareMonitorSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IHardwareMonitor, LinuxHardwareMonitorOld>();

        return services;
    }
}
