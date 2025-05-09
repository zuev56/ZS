using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Zs.Common.Extensions;

public static class StringExtensions
{
    private static readonly JsonSerializerOptions PrettyJsonSerializerOptions = new ()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string ReplaceEndingWithThreeDots(this string value, int maxStringLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        if (maxStringLength < 4)
            maxStringLength = 4;

        return value.Length < maxStringLength
            ? value
            : value[..(maxStringLength - 3)].TrimEnd() + "...";
    }

    public static string FirstCharToUpper(this string value)
        => value switch
        {
            null => throw new ArgumentNullException(nameof(value)),
            "" => throw new ArgumentException($"{nameof(value)} cannot be empty", nameof(value)),
            _ => string.Concat(value.First().ToString().ToUpper(), value.AsSpan(1))
        };

    public static string GetMd5Hash(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        using var md5Hash = MD5.Create();
        var bytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(value));
        var sb = new StringBuilder();

        foreach (var @byte in bytes)
            sb.Append(@byte.ToString("x2"));

        return sb.ToString();
    }

    /// <summary>Split a string into parts of specific length</summary>
    /// <param name="partLength">Length of one part</param>
    /// <returns><see cref="IEnumerable&lt;string&gt;"/></returns>
    public static IEnumerable<string> SplitIntoParts(this string value, int partLength)
    {
        if (partLength <= 0)
            throw new ArgumentException($"{nameof(partLength)} has to be positive.", nameof(partLength));

        if (value == string.Empty)
            yield return string.Empty;

        for (var i = 0; i < value.Length; i += partLength)
        {
            var length = Math.Min(partLength, value.Length - i);
            yield return value.Substring(i, length);
        }
    }

    public static string? WithoutDigits(this string? value)
        => value == null ? null : Regex.Replace(value, @"[\d]", "");

    public static string? ReplaceEmojiWithX(this string? value)
        => value == null ? null : Regex.Replace(value, @"\p{Cs}", "[x]");

    /// <summary> Sort parameters and make pretty JSON string </summary>
    public static string NormalizeJsonString(this string json)
    {
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.Object:
            {
                var normalizedJsonElement = SortPropertiesAlphabetically(jsonElement);
                return JsonSerializer.Serialize(normalizedJsonElement, PrettyJsonSerializerOptions);
            }
            case JsonValueKind.Array:
            {
                var jsonArray = jsonElement.EnumerateArray().ToList();

                for (var i = 0; i < jsonArray.Count; i++)
                {
                    var normalizedItem = jsonArray[i].ToString().NormalizeJsonString();
                    jsonArray[i] = JsonSerializer.Deserialize<JsonElement>(normalizedItem);
                }
                return JsonSerializer.Serialize(jsonArray, PrettyJsonSerializerOptions);
            }
            default:
                return json;
        }
    }

    private static JsonElement SortPropertiesAlphabetically(JsonElement original)
    {
        static bool IsObjectOrArray(JsonElement original) => original.ValueKind is JsonValueKind.Object or JsonValueKind.Array;

        switch (original.ValueKind)
        {
            case JsonValueKind.Object:
            {
                var properties = original.EnumerateObject().OrderBy(p => p.Name).ToList();
                var keyValuePairs = new Dictionary<string, JsonElement>(properties.Count);
                foreach (var property in properties)
                {
                    if (IsObjectOrArray(property.Value))
                    {
                        var jsonElement = SortPropertiesAlphabetically(property.Value);
                        keyValuePairs.Add(property.Name, jsonElement);
                    }
                    else
                    {
                        keyValuePairs.Add(property.Name, property.Value);
                    }
                }
                var bytes = JsonSerializer.SerializeToUtf8Bytes(keyValuePairs);
                return JsonDocument.Parse(bytes).RootElement.Clone();
            }
            case JsonValueKind.Array:
            {
                var elements = original.EnumerateArray().ToList();
                if (elements.Count > 0 && IsObjectOrArray(elements[0]))
                {
                    for (var i = 0; i < elements.Count; i++)
                    {
                        elements[i] = SortPropertiesAlphabetically(elements[i]);
                    }
                }

                var bytes = JsonSerializer.SerializeToUtf8Bytes(elements);
                return JsonDocument.Parse(bytes).RootElement.Clone();
            }
            default:
                return original;
        }
    }

    public static bool IsValidIp(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value.StartsWith(' ') || value.EndsWith(' '))
        {
            return false;
        }

        var splitValues = value.Split('.');
        return splitValues.Length == 4 && splitValues.All(r => byte.TryParse(r, out _));
    }

    public static bool IsConnectionString(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var lowerValue = value.ToLowerInvariant();

        var isPostgres =
            lowerValue.Contains("host=") ||
            lowerValue.Contains("port=") ||
            lowerValue.Contains("database=") ||
            lowerValue.Contains("user=") ||
            lowerValue.Contains("password=");

        var isSqlite =
            lowerValue.Contains("data source=") ||
            lowerValue.Contains("cache=") ||
            lowerValue.Contains("mode=");

        return isPostgres || isSqlite;
    }
}
