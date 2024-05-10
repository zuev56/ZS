using System;
using FluentAssertions;
using Xunit;
using Zs.Common.Models;

namespace Zs.Common.UnitTests;

public sealed class ResultTests
{
    [Fact]
    public void Should_ImplicitlyConvert_FaultToResult()
    {
        var fault = Fault.Unknown;
        Func<Fault, Result> implicitConverter = static f => f;

        var result = implicitConverter.Invoke(fault);

        result.Should().NotBeNull();
        result.Successful.Should().BeFalse();
        fault.Should().NotBeNull();
        fault.Code.Should().Be(Fault.Unknown.Code);
    }

    [Fact]
    public void Should_ImplicitlyConvert_ValueToParametrizedResult()
    {
        const int value = 12345;
        Func<int, Result<int>> implicitConverter = static v => v;

        var result = implicitConverter.Invoke(value);

        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Should_ImplicitlyConvert_FaultToParametrizedResult()
    {
        var fault = Fault.Unknown;
        Func<Fault, Result<int?>> implicitConverter = static f => f;

        var result = implicitConverter.Invoke(fault);

        result.Should().NotBeNull();
        result.Successful.Should().BeFalse();
        fault.Should().NotBeNull();
        fault.Code.Should().Be(Fault.Unknown.Code);
    }
}
