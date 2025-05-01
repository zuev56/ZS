using System.Collections.Generic;
using System.Linq;

namespace Zs.Parser.EspMeteo.Models;

public sealed class EspMeteo
{
    public string Uri { get; }
    public IReadOnlyList<Sensor> Sensors { get; }

    public EspMeteo(string uri, IEnumerable<Sensor> sensors)
    {
        Uri = uri;
        Sensors = sensors.ToList();
    }
}
