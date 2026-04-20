using System.Text.Json.Serialization;

namespace Zs.Home.AIAgent.Worker.Plugins.Devices.Models;

public sealed class DiscreteControl : Control
{
    [JsonPropertyName("is_on")]
    public bool? IsOn { get; set; }
}
