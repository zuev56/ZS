using System;

namespace Zs.Common.Models;

public sealed class DateTimeRange
{
    public DateTime Start { get; }
    public DateTime End { get; }

    public DateTimeRange(DateTime start)
    {
        Start = start;
    }

    public DateTimeRange(DateTime start, DateTime end)
        : this(start)
    {
        End = end;
    }
}

public static class DateTimeRangeExtensions
{
    public static DateTimeRange LastHourUtc => new(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);
    public static DateTimeRange Last6HoursUtc => new(DateTime.UtcNow.AddHours(-6), DateTime.UtcNow);
    public static DateTimeRange Last12HoursUtc => new(DateTime.UtcNow.AddHours(-12), DateTime.UtcNow);
    public static DateTimeRange LastDayUtc => new(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
    public static DateTimeRange LastWeekUtc => new(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
    public static DateTimeRange LastMonthUtc => new(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);
    public static DateTimeRange LastYearUtc => new(DateTime.UtcNow.AddYears(-1), DateTime.UtcNow);
}
