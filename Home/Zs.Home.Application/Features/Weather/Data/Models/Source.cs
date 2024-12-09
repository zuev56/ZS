using System;
using System.Collections.Generic;

namespace Zs.Home.Application.Features.Weather.Data.Models;

public sealed class Source
{
    public required short Id { get; set; }
    public required short PlaceId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public Place Place { get; set; } = null!;
    public ICollection<WeatherData>? WeatherData { get; set; }
}
