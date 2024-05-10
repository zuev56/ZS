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