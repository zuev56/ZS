using System.Text.Json.Serialization;

namespace Zs.Home.AIAgent.Worker.Plugins.Music;

public sealed class MusicTrack
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string SearchText { get; set; } = string.Empty;

    [JsonIgnore]
    public DateTime UpdatedAt { get; set; }

    public float? Rank { get; set; }
}
