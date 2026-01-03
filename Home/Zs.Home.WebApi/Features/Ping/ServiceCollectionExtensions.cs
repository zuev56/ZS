using Zs.Home.WebApi.Features.Ping.Models;

namespace Zs.Home.WebApi.Features.Ping;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPingChecker(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PingCheckerSettings>()
            .Bind(configuration.GetSection(PingCheckerSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IPingChecker, PingChecker>();

        return services;
    }
}
