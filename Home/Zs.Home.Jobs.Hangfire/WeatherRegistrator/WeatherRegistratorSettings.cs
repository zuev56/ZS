using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zs.Home.Jobs.Hangfire.Hangfire;

namespace Zs.Home.Jobs.Hangfire.WeatherRegistrator;

public sealed class WeatherRegistratorSettings : ICronSettings
{
    internal const string SectionName = "WeatherRegistrator";

    [Required]
    public required string CronExpression { get; init; }

    [Required]
    public required IReadOnlyList<Place> Places { get; init; }

    [Required]
    public required IReadOnlyList<Sensor> Sensors { get; init; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Place
{
    [Required]
    public required short Id { get; init; }

    [Required]
    public required string Name { get; init; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Sensor
{
    [Required]
    public required short Id { get; init; }

    [Required]
    public required short PlaceId { get; init; }

    [Required]
    public required string Name { get; init; }

    [Required]
    public required string Uri { get; init; }

    public string? Except { get; set; }
}
