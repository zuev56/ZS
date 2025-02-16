using System;

namespace Zs.Home.ClientApp.Common;

public static class TimeSpanExtensions
{
    public static string ToTimeAgo(this TimeSpan time)
        => time.TotalHours >= 24
            ? $"{Math.Round(time.TotalHours)}d ago"
            : time.TotalMinutes > 99
                ? $"{Math.Round(time.TotalHours)}h ago"
                : time.TotalMinutes >= 1
                    ? $"{Math.Round(time.TotalMinutes)}m ago"
                    : "now";
}
