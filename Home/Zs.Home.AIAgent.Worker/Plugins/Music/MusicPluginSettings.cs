using Zs.Home.AIAgent.Worker.Models;

namespace Zs.Home.AIAgent.Worker.Plugins.Music;

public sealed class MusicPluginSettings
{
    public const string PluginName = "Music";
    public const string SectionName = $"{Constants.Plugins}:{PluginName}";

    public string ConnectionString { get; set; } = null!;
    public string[] Extensions { get; set; } = [];
    public string[] Paths { get; set; } = [];
}
