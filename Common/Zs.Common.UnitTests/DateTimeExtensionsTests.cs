using System;
using FluentAssertions;
using Xunit;
using Zs.Common.Extensions;

namespace Zs.Common.UnitTests;

public sealed class DateTimeExtensionsTests
{
    private const string Format = "dd.MM.yyyy HH:mm:ss";

    [Theory]
    [InlineData("01.01.2000 01:00:00", "01.01.2000 02:00:00")]
    [InlineData("01.01.2000 01:23:45", "01.01.2000 02:00:00")]
    [InlineData("01.01.2000 13:23:45", "01.01.2000 14:00:00")]
    [InlineData("01.01.2000 13:00:00", "01.01.2000 14:00:00")]
    [InlineData("01.01.2000 23:00:00", "02.01.2000 00:00:00")]
    [InlineData("01.01.2000 23:23:45", "02.01.2000 00:00:00")]
    public void NextHour_ShouldReturn_BeginningOfTheNextHour(string sourceDate, string expectedDate)
    {
        var sourceDateTime = DateTime.ParseExact(sourceDate, Format, null);
        var expectedDateTime = DateTime.ParseExact(expectedDate, Format, null);

        var nextHour = sourceDateTime.NextHour();

        nextHour.Should().Be(expectedDateTime);
    }

    [Theory]
    [InlineData(-1, "31.12.1969 23:59:59")]
    [InlineData(1, "01.01.1970 00:00:01")]
    [InlineData(946684800, "01.01.2000 00:00:00")]
    [InlineData(1677591991, "28.02.2023 13:46:31")]
    public void FromUnixEpoch_ShouldConvert_SecondsToDateTime(int unixTime, string expectedDate)
    {
        var expectedDateTime = DateTime.ParseExact(expectedDate, Format, null);

        var fromUnixEpoch = unixTime.FromUnixEpoch();

        fromUnixEpoch.Should().Be(expectedDateTime);
    }

    [Theory]
    [InlineData(-1, "31.12.1969 23:59:59")]
    [InlineData(1, "01.01.1970 00:00:01")]
    [InlineData(946684800, "01.01.2000 00:00:00")]
    [InlineData(1677591991, "28.02.2023 13:46:31")]
    public void ToUnixEpoch_ShouldConvert_DateTimeToSeconds(int expectedUnixTime, string sourceDate)
    {
        var sourceDateTime = DateTime.ParseExact(sourceDate, Format, null);

        var unixTime = sourceDateTime.ToUnixEpoch();

        unixTime.Should().Be(expectedUnixTime);
    }
}
