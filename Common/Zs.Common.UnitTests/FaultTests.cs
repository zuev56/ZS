using System;
using FluentAssertions;
using Xunit;
using Zs.Common.Models;

namespace Zs.Common.UnitTests;

public sealed class FaultTests
{
    [Fact]
    public void Should_ImplicitlyConvert_StringToFault()
    {
        const string faultMessage = "FaultMessage";
        Func<string, Fault> implicitConverter = static s => s;

        var fault = implicitConverter.Invoke(faultMessage);

        fault.Should().NotBeNull();
        fault.Code.Should().Be(Fault.Unknown.Code);
        fault.Message.Should().Be(faultMessage);
    }
}
