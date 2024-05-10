using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;
using Zs.Common.Extensions;

namespace Zs.Common.UnitTests;

public sealed class ConfigurationExtensionsTests
{
    [Fact]
    public void ContainsKey_ShouldReturnTrue_When_KeyExists()
    {
        var existingKey = "ExistingKey";
        var configurationSource = new Dictionary<string, string> { {existingKey, "SomeValue"} };
        var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationSource)
                .Build();

        var contains = configuration.ContainsKey(existingKey);

        contains.Should().BeTrue();
    }

    [Fact]
    public void ContainsKey_ShouldReturnFalse_When_KeyDoesNotExist()
    {
        var emptyConfigurationSource = new Dictionary<string, string>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(emptyConfigurationSource)
            .Build();

        var contains = configuration.ContainsKey("non-existent key");

        contains.Should().BeFalse();
    }
}
