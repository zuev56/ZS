using System.Collections.Generic;
using System.Linq;

namespace Zs.Parser.EspMeteo.Models;

public class Sensor
{
    public string Name { get; }
    public IReadOnlyList<Parameter> Parameters { get; }

    public Sensor(string name, IEnumerable<Parameter> parameters)
    {
        Name = name;
        Parameters = parameters.ToList();
    }
}