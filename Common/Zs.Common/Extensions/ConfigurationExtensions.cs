using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static System.StringComparison;

namespace Zs.Common.Extensions;

public static class ConfigurationExtensions
{
    public static bool ContainsKey(this IConfiguration configuration, string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        return configuration.GetChildren().Any(item => item.Key == key);
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
            if (File.Exists(arg) && Path.GetExtension(arg).Equals(".json", InvariantCultureIgnoreCase))
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
}
