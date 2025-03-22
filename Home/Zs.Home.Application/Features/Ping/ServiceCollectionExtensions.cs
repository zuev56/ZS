using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zs.Home.Application.Features.Ping;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPingChecker(this IServiceCollection services, IConfiguration configuration)
        => AddPingChecker<PingCheckerSettings>(services, configuration);

    public static IServiceCollection AddPingChecker<TSettings>(this IServiceCollection services, IConfiguration configuration)
        where TSettings : PingCheckerSettings
    {
        // TODO: Настройки теперь не нужны для запуска сервиса,
        //       надо вынести их определение в конкретные места или убрать их валидацию отсюда
        services.AddOptions<TSettings>()
            .Bind(configuration.GetSection(PingCheckerSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IPingChecker, PingChecker>();

        return services;
    }
}
