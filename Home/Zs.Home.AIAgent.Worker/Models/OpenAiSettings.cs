namespace Zs.Home.AIAgent.Worker.Models;

public sealed class OpenAiSettings
{
    public const string SectionName = "OpenAI";
    public required string Url { get; set; }
    public required string ModelName { get; set; }
    public string? ApiKey { get; set; }
    public bool StreamingMode { get; set; } = true;
    public bool SaveAndUseHistory { get; set; } = true;
    public string? TavilyTextSearchApiKey { get; set; }
}
