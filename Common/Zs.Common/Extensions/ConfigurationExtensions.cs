using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Zs.Common.Extensions;

public static class ConfigurationExtensions
{
    public static bool ContainsKey(this IConfiguration configuration, string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        return configuration.GetChildren().Any(item => item.Key == key);
    }
}