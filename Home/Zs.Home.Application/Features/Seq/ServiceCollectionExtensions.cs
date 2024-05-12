using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Common.Services.Logging.Seq;

namespace Zs.Home.Application.Features.Seq;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSeq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SeqSettings2>()
            .Bind(configuration.GetSection(SeqSettings2.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ISeqService, SeqService>(static provider =>
        {
            var options = provider.GetRequiredService<IOptions<SeqSettings2>>().Value;
            var logger = provider.GetRequiredService<ILogger<SeqService>>();

            return new SeqService(options.Url, options.Token, logger);
        });

        services.AddSingleton<ISeqEventsInformer, SeqEventsInformer>();

        return services;
    }
}
