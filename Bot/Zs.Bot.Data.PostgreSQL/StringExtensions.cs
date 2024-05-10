using System;
using System.Collections.Generic;
using System.Linq;
using Zs.Common.Exceptions;

namespace Zs.Bot.Data.PostgreSQL;

internal static class StringExtensions
{
    internal static string ToPostgresqlJsonPath(this string jsonPath)
    {
        EnsureJsonPathIsValid(jsonPath);

        var jsonPathParts = jsonPath.Split('.')[1..];
        var pgJsonPathParts = new List<string>();
        foreach (var jsonPathPart in jsonPathParts.Reverse())
        {
            var pgJsonPathPart = pgJsonPathParts.Any() ? $"-> '{jsonPathPart}'" : $"->> '{jsonPathPart}'";
            pgJsonPathParts.Insert(0, pgJsonPathPart);
        }

        return string.Join(' ', pgJsonPathParts);
    }

    private static void EnsureJsonPathIsValid(string jsonPath)
    {
        var isValid = jsonPath.StartsWith("$.")
                      && jsonPath.Length > 2
                      && !jsonPath.EndsWith(".", StringComparison.InvariantCultureIgnoreCase)
                      && jsonPath.Trim() == jsonPath;

        if (!isValid)
            throw new FaultException($"Invalid jsonPath: {jsonPath}");
    }

    internal static string ToRawDataColumnJsonPath(this string pgJsonPath, object value)
    {
        var columnName = "raw_data";
        return value switch
        {
            string => $"{columnName} {pgJsonPath}",
            DateTime => $"cast({columnName} {pgJsonPath} as timestamptz)",
            int => $"cast({columnName} {pgJsonPath} as int)",
            long => $"cast({columnName} {pgJsonPath} as bigint)",
            bool => $"cast({columnName} {pgJsonPath} as boolean)",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

}