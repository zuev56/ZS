using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zs.Home.Application.Features.Seq;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSeqLogAnalyzer(this IServiceCollection services, IConfiguration configuration)
        => AddSeqLogAnalyzer<SeqSettings>(services, configuration);

    public static IServiceCollection AddSeqLogAnalyzer<TSettings>(this IServiceCollection services, IConfiguration configuration)
        where TSettings : SeqSettings
    {
        services.AddOptions<TSettings>()
            .Bind(configuration.GetSection(SeqSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<SeqSettings>()
            .Bind(configuration.GetSection(SeqSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ILogAnalyzer, SeqLogAnalyzer>();

        return services;
    }
}
