using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zs.Home.Application.Features.Weather.Data;
using Zs.Home.Jobs.Hangfire.Hangfire;
using Zs.Home.Jobs.Hangfire.HardwareAnalyzer;
using Zs.Home.Jobs.Hangfire.LogAnalyzer;
using Zs.Home.Jobs.Hangfire.Ping;
using Zs.Home.Jobs.Hangfire.WeatherAnalyzer;
using Zs.Home.Jobs.Hangfire.WeatherRegistrator;
using Zs.Parser.EspMeteo;
using static Zs.Home.Jobs.Hangfire.Constants;

namespace Zs.Home.Jobs.Hangfire.Extensions;

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

    internal static IServiceCollection AddJobConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<LogAnalyzerSettings>()
            .Bind(configuration.GetSection(LogAnalyzerSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<HardwareAnalyzerSettings>()
            .Bind(configuration.GetSection(HardwareAnalyzerSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<PingCheckerSettings>()
            .Bind(configuration.GetSection(PingCheckerSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<WeatherAnalyzerSettings>()
            .Bind(configuration.GetSection(WeatherAnalyzerSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
