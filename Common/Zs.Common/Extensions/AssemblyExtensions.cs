using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Zs.Common.Extensions;

public static class AssemblyExtensions
{
    public static string? ReadResource(this Assembly assembly, string resourceName)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrEmpty(resourceName);

        var resourcePath = assembly
            .GetManifestResourceNames()
            .Single(str => str.EndsWith(resourceName));

        using var stream = assembly.GetManifestResourceStream(resourcePath);
        if (stream is null)
            return null;

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}