using System.Collections.Generic;

namespace Zs.Parser.EspMeteo.Models;

public sealed record Sensor
{
    public required string Name { get; init; }
    public required IReadOnlyList<Parameter> Parameters { get; init; }
}
