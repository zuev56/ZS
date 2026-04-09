using System;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Zs.Common.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureExternalAppConfiguration(this IHostBuilder builder, string[] args, Assembly assembly)
        => builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.TryLoadConfigurationJsonFromArguments(assembly, args);

            var configFiles = configurationBuilder.GetAppliedConfigurationFileNames();
            Console.WriteLine($"Applied configuration files: {string.Join(", ", configFiles)}");
        });

    public static IHostApplicationBuilder ConfigureExternalAppConfiguration(this IHostApplicationBuilder builder, string[] args, Assembly assembly)
    {
        builder.Configuration.TryLoadConfigurationJsonFromArguments(assembly, args);

        var configFiles = builder.Configuration.GetAppliedConfigurationFileNames();
        Console.WriteLine($"Applied configuration files: {string.Join(", ", configFiles)}");

        return builder;
    }

    public static IHostBuilder ConfigureTimezone(this IHostBuilder builder)
    {
        var timeZone = Environment.GetEnvironmentVariable("TimeZone");

        SetCurrentCulture(timeZone);

        return builder;
    }


    public static IHostApplicationBuilder ConfigureTimezone(this IHostApplicationBuilder builder)
    {
        var timeZone =  builder.Configuration["TimeZone"];

        SetCurrentCulture(timeZone);

        return builder;
    }

    private static void SetCurrentCulture(string? timeZone)
    {
        if (!string.IsNullOrWhiteSpace(timeZone))
            CultureInfo.CurrentCulture = new CultureInfo(timeZone);

        Console.WriteLine($"\"TimeZone\": {timeZone}, CurrentCulture: {CultureInfo.CurrentCulture}");
    }
}
