using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

    public static string ToFormattedString(this IConfiguration configuration)
    {
        var sb = new StringBuilder();
        BuildConfigurationTree(sb, configuration, string.Empty);
        return sb.ToString();

        static void BuildConfigurationTree(StringBuilder sb, IConfiguration config, string indent)
        {
            foreach (var child in config.GetChildren())
            {
                sb.Append(indent);
                sb.Append(child.Key);

                var value = PrepareValue(child);
                if (value != null)
                {
                    sb.Append(" = ");
                    sb.AppendLine(value);
                }
                else
                {
                    sb.AppendLine();
                }

                BuildConfigurationTree(sb, child, indent + "  ");
            }
        }
    }

    private static string? PrepareValue(IConfigurationSection section)
    {
        var secretIdentifiers = new [] { "secret", "key", "token", "password" };

        if (section.Value.IsConnectionString() && section.Value!.Contains("password=", InvariantCultureIgnoreCase))
        {
            var passwordStart = section.Value.IndexOf("password=", InvariantCultureIgnoreCase) + 9;
            var passwordEnd = passwordStart + section.Value[passwordStart..].IndexOf(';');
            var hiddenPassword = new string('*', passwordEnd - passwordStart + Random.Shared.Next(-1, 5));
            return section.Value[..passwordStart] + hiddenPassword + section.Value[passwordEnd..];
        }

        foreach (var secret in secretIdentifiers)
        {
            if (section.Value != null && section.Key.Contains(secret, InvariantCultureIgnoreCase))
            {
                var length = section.Value.Length + Random.Shared.Next(-3, 3);
                return new string('*', length);
            }
        }

        return section.Value;
    }
}
