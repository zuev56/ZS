using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zs.Home.Application.Features.Ping;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPingChecker(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PingCheckerSettings>()
            .Bind(configuration.GetSection(PingCheckerSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<PingChecker>();

        return services;
    }
}
