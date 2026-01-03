using System;

namespace Zs.Home.ClientApp.Common;

public static class TimeSpanExtensions
{
    public static string ToTimeAgo(this TimeSpan time)
        => time.TotalHours >= 23.7
            ? $"{Math.Round(time.TotalDays)}d ago"
            : time.TotalMinutes > 99
                ? $"{Math.Round(time.TotalHours)}h ago"
                : time.TotalMinutes >= 1
                    ? $"{Math.Round(time.TotalMinutes)}m ago"
                    : "now";
}
