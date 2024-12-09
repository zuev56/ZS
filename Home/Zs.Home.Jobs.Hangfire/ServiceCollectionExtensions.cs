using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zs.Home.Application.Features.Weather.Data;
using Zs.Home.Jobs.Hangfire.Hangfire;
using Zs.Home.Jobs.Hangfire.WeatherRegistrator;
using Zs.Parser.EspMeteo;
using static Zs.Home.Jobs.Hangfire.Constants;

namespace Zs.Home.Jobs.Hangfire;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddWeatherRegistrator(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<WeatherRegistratorSettings>()
            .Bind(configuration.GetSection(WeatherRegistratorSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<EspMeteoParser>();

        var connectionString = configuration.GetConnectionString(HomeDbConnectionString);
        services.AddDbContextFactory<WeatherRegistratorDbContext>(
            options => options.UseNpgsql(connectionString));

        return services;
    }

    internal static IServiceCollection AddHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(x =>
        {
            x.UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(config =>
                {
                    config.UseNpgsqlConnection(configuration.GetConnectionString(HomeDbConnectionString));
                }, new PostgreSqlStorageOptions {SchemaName = HangfireSchema, PrepareSchemaIfNecessary = true});
        });

        var connectionString = configuration.GetConnectionString(HomeDbConnectionString);
        services.AddDbContextFactory<HangfireDbContext>(
            options => options.UseNpgsql(connectionString));

        return services.AddHangfireServer();
    }
}
