using System.Text.Json.Serialization;

namespace Zs.Home.AIAgent.Worker.Plugins.Devices.Models;

public sealed class AnalogControl : Control
{
    [JsonPropertyName("value")]
    public double? Value { get; set; }
}
