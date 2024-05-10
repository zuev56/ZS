using System;
using System.Collections.Generic;
using System.Linq;

namespace Zs.Bot.Data.SQLite.IntegrationTests;

public static class EnumerableExtensions
{
    public static TValue GetRandomValue<TValue>(this IEnumerable<TValue> items)
    {
        var array = items.ToArray();
        var randomIndex = Random.Shared.Next(0, array.Length);
        return array[randomIndex];
    }
}