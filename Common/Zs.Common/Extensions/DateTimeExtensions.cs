using System;

namespace Zs.Common.Extensions;

public static class DateTimeExtensions
{
    private static readonly DateTime UnixEpoch = new (1970,1,1,0,0,0, DateTimeKind.Utc);

    public static DateTime NextHour(this DateTime dateTime)
    {
        var nextHour = dateTime.Hour < 23
            ? dateTime.Date + TimeSpan.FromHours(1)
            : dateTime.Date + TimeSpan.FromDays(1);

        while (dateTime >= nextHour)
        {
            nextHour += TimeSpan.FromHours(1);
        }

        return nextHour;
    }

    public static DateTime FromUnixEpoch(this int seconds) => UnixEpoch + TimeSpan.FromSeconds(seconds);

    public static int ToUnixEpoch(this DateTime dateTime) => (int)dateTime.Subtract(UnixEpoch).TotalSeconds;
}