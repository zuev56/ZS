using System.Text.Json.Serialization;

namespace Zs.Home.AIAgent.Worker.Plugins.Devices.Models;

public abstract class Control
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }
}
