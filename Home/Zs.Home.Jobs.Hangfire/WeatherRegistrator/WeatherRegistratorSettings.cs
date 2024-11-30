using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Jobs.Hangfire.WeatherRegistrator;

public sealed class WeatherRegistratorSettings
{
    public const string SectionName = "WeatherRegistrator";

    [Required]
    public required string CronExpression { get; set; }

    [Required]
    public required IReadOnlyList<Place> Places { get; set; }

    [Required]
    public required IReadOnlyList<Sensor> Sensors { get; set; }
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
}
