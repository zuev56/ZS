using System;
using System.Collections.Generic;

namespace Zs.Home.Application.Features.Weather.Data.Models;

public sealed class Place
{
    public required short Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Source>? Sources { get; set; }
}
