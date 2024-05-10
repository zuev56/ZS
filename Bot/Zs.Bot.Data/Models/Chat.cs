namespace Zs.Bot.Data.Models;

public sealed class Chat : DbEntity
{
    public string Name { get; set; } = null!;
    public ChatType Type { get; set; }

    public override string ToString() => $"{Name} ({Id} | {Type})";
}