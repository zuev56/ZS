using FluentAssertions;
using Xunit;
using Zs.Common.Extensions;

namespace Zs.Common.UnitTests;

public sealed class LongExtensionsTests
{
    [Theory]
    [InlineData(1, "1 Byte")]
    [InlineData(1020, "1020 Bytes")]
    [InlineData(1025, "1 KB")]
    [InlineData(2000000, "1.91 MB")]
    [InlineData(3000000000, "2.79 GB")]
    [InlineData(4000000000000, "3.64 TB")]
    public void BytesAmountToSizeString_ShouldReturn_ExpectedValue(long bytesAmount, string expectedValue)
    {
        var size = bytesAmount.BytesAmountToSizeString();

        size.Should().Be(expectedValue);
    }
}
