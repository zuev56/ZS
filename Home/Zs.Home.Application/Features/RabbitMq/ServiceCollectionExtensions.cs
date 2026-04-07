using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Zs.Home.Application.Features.RabbitMq;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RabbitMqSettings>()
            .Bind(configuration.GetSection(RabbitMqSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ConnectionFactory>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

            return new ConnectionFactory
            {
                HostName = settings.HostName,
                Port = settings.Port,
                UserName = settings.UserName,
                Password = settings.Password
            };
        });

        return services;
    }
}
