using System;
using System.Linq;

namespace Zs.Common.Extensions;

public static class TimeSpanExtensions
{
    public static TimeSpan Microseconds(this int value) => TimeSpan.FromMicroseconds(value);
    public static TimeSpan Milliseconds(this int value) => TimeSpan.FromMilliseconds(value);
    public static TimeSpan Seconds(this int value) => TimeSpan.FromSeconds(value);
    public static TimeSpan Minutes(this int value) => TimeSpan.FromMinutes(value);
    public static TimeSpan Hours(this int value) => TimeSpan.FromHours(value);
    public static TimeSpan Days(this int value) => TimeSpan.FromDays(value);
    public static TimeSpan Ticks(this int value) => TimeSpan.FromTicks(value);

    public static TimeSpan Microseconds(this double value) => TimeSpan.FromMicroseconds(value);
    public static TimeSpan Milliseconds(this double value) => TimeSpan.FromMilliseconds(value);
    public static TimeSpan Seconds(this double value) => TimeSpan.FromSeconds(value);
    public static TimeSpan Minutes(this double value) => TimeSpan.FromMinutes(value);
    public static TimeSpan Hours(this double value) => TimeSpan.FromHours(value);
    public static TimeSpan Days(this double value) => TimeSpan.FromDays(value);

    public static TimeSpan Ticks(this long value) => TimeSpan.FromTicks(value);

    public static string ToDayHHmmss(this TimeSpan timeSpan)
    {
        // TODO: handle different languages
        var days = "";
        if (timeSpan.TotalDays >= 1)
        {
            days = $"{Math.Floor(timeSpan.TotalDays):0}";
            var lastDayDigit = (int)char.GetNumericValue(days.Last());
            var lastTwoDayDigits = (int)(timeSpan.TotalDays % 100);

            days += lastDayDigit switch
            {
                1 when lastTwoDayDigits != 11 => " день ",
                > 1 and < 5 when lastTwoDayDigits is < 11 or > 14 => " дня ",
                _ => " дней "
            };
        }

        return $"{days}{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
    }
}