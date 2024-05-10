using System;
using FluentAssertions;
using Xunit;
using Zs.Common.Extensions;

namespace Zs.Common.UnitTests;

public sealed class TimeSpanExtensionsTests
{
    [Theory]
    [InlineData(1, "00:00:01")]
    [InlineData(86400, "1 день 00:00:00")]
    [InlineData(326871, "3 дня 18:47:51")]
    [InlineData(965972, "11 дней 04:19:32")]
    public void ToDayHHmmss_ShouldReturn_ExpectedValue(long seconds, string expectedValue)
    {
        var timeSpan = TimeSpan.FromSeconds(seconds);

        var timeString = timeSpan.ToDayHHmmss();

        timeString.Should().Be(expectedValue);
    }
}
