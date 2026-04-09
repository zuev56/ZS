using System;
using System.Reflection;
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
}
