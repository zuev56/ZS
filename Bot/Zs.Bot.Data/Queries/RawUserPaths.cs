namespace Zs.Bot.Data.Queries;

public record RawUserPaths
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? IsBot { get; init; }

    public void Deconstruct(out string id, out string name, out string? isBot)
    {
        id = Id;
        name = Name;
        isBot = IsBot;
    }
}