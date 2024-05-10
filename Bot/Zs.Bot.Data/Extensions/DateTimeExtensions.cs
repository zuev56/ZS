using System;

namespace Zs.Bot.Data.Extensions;

public static class DateTimeExtensions
{
    public static string ToJsonDate(this DateTime dateTime)
        => $"{dateTime:yyyy-MM-dd HH:mm:ss:fff}";
}