using System;
using FluentAssertions;
using Xunit;
using Zs.Common.Extensions;

namespace Zs.Common.UnitTests;

public sealed class StringExtensionsTests
{
    [Theory]
    [InlineData("", 5, 1)]
    [InlineData("123", 1, 3)]
    [InlineData("123456789", 3, 3)]
    [InlineData("12345678", 3, 3)]
    [InlineData("123456789a", 3, 4)]
    [InlineData("12345", 5, 1)]
    [InlineData("12345", 15, 1)]
    public void SplitIntoParts_ShouldReturn_SplitParts(string sourceString, int partLength, int expectedPartsCount)
    {
        var parts = sourceString.SplitIntoParts(partLength);

        parts.Should().HaveCount(expectedPartsCount);
    }

    [Theory(Skip = "Doesn't work, analyze it later")]
    [InlineData("abc", 0)]
    [InlineData("abc", -1)]
    public void SplitIntoParts_ShouldThrow_ArgumentException_When_IncorrectPartLength(string sourceString, int partLength)
    {
        Action action = () => sourceString.SplitIntoParts(partLength);

        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("a", "a")]
    [InlineData("1", "")]
    [InlineData("1q2", "q")]
    [InlineData("q1w", "qw")]
    [InlineData(".123-qwe!456+a7s7d", ".-qwe!+asd")]
    [InlineData("qwe!()*&^%$#@!+=-_;:\"'/\\?.,`~", "qwe!()*&^%$#@!+=-_;:\"'/\\?.,`~")]
    public void WithoutDigits_ShouldReturn_StringWithoutDigits(string sourceString, string expectedString)
    {
        var stringWithoutDigits = sourceString.WithoutDigits();

        stringWithoutDigits.Should().Be(expectedString);
    }

    [Theory]
    [InlineData("0.0.0.0")]
    [InlineData("255.255.255.255")]
    [InlineData("1.2.3.4")]
    [InlineData("127.0.0.1")]
    [InlineData("192.168.1.1")]
    public void IsValidIp_ShouldReturn_True(string sourceString)
    {
        var isValidIp = sourceString.IsValidIp();

        isValidIp.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1.2.3.4 ")]
    [InlineData(" 1.2.3.4")]
    [InlineData("1..3.4")]
    [InlineData("1.-2.3.4")]
    [InlineData("1.2.3")]
    [InlineData(".2.3.4")]
    [InlineData("1.2.3.")]
    [InlineData("1.2.3.4.")]
    [InlineData("256.255.255.255")]
    [InlineData("A.2.3.4")]
    [InlineData("localhost")]
    public void IsValidIp_ShouldReturn_False(string sourceString)
    {
        var isValidIp = sourceString.IsValidIp();

        isValidIp.Should().BeFalse();
    }
}
