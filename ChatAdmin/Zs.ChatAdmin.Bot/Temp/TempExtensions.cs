using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// TODO: Т.к. версия базовых библиотек здесь очень старая и берётся из нугета, приходится т.о. добавлять обновления

namespace Zs.Common.TempExtensions;

public static class TempExtensions
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

    /// <summary>
    /// Загрузка файла конфигураци из аргументов командной строки.
    /// </summary>
    /// <remarks>
    /// В параметрах можно передать путь к конкретному файлу конфигурации.
    /// Или указать каталог, в котором будет искаться json-файл по имени сборки.
    /// </remarks>
    public static bool TryLoadConfigurationJsonFromArguments(
        this IConfigurationBuilder configuration, Assembly assembly, string[]? args)
    {
        if (args?.Any() != true)
            return false;

        var loaded = false;
        var assemblyName = assembly.GetName().Name!;

        foreach (var arg in args.Distinct().Where(a => !string.IsNullOrWhiteSpace(a)))
        {
            if (File.Exists(arg) && Path.GetExtension(arg).Equals(".json", StringComparison.InvariantCultureIgnoreCase))
            {
                configuration.AddJsonFile(arg);
                loaded = true;
            }

            var configFilePath = Path.Combine(arg, $"{assemblyName}.json");
            if (File.Exists(configFilePath))
            {
                configuration.AddJsonFile(configFilePath);
                loaded = true;
            }
        }

        return loaded;
    }

    public static List<string> GetAppliedConfigurationFileNames(this IConfigurationBuilder configuration)
        => configuration.Sources
            .Where(s => s is FileConfigurationSource)
            .Select(s => ((FileConfigurationSource)s).Path)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList()!;

    public static void LogProgramStartup(this ILogger logger)
    {
        logger.LogWarning("-! Starting {ProcessName} (MachineName: {MachineName}, OS: {OS}, User: {User}, ProcessId: {ProcessId})",
            Process.GetCurrentProcess().MainModule?.ModuleName,
            Environment.MachineName,
            Environment.OSVersion,
            Environment.UserName,
            Environment.ProcessId);
    }

}
