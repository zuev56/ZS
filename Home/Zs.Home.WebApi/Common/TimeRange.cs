using Zs.Common.Models;

namespace Zs.Home.WebApi.Common;

public enum TimeRange
{
    OneHour = 1,
    SixHours,
    TwelveHours,
    OneDay,
    OneWeek
}

internal static class TimeRangeExtensions
{
    public static DateTimeRange ToDateTimeRange(this TimeRange timeRange)
        => timeRange switch
        {
            TimeRange.OneHour => new DateTimeRange(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow),
            TimeRange.SixHours => new DateTimeRange(DateTime.UtcNow.AddHours(-6), DateTime.UtcNow),
            TimeRange.TwelveHours => new DateTimeRange(DateTime.UtcNow.AddHours(-12), DateTime.UtcNow),
            TimeRange.OneDay => new DateTimeRange(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow),
            TimeRange.OneWeek => new DateTimeRange(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow),
            _ => throw new ArgumentOutOfRangeException(nameof(timeRange), timeRange, null)
        };
}
