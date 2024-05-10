namespace Zs.Bot.Data.Queries;

public record RawChatPaths
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;

    public void Deconstruct(out string id, out string name)
    {
        id = Id;
        name = Name;
    }
}