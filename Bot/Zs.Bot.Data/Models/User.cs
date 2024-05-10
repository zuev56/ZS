namespace Zs.Bot.Data.Models;

public sealed class User : DbEntity
{
    public string? UserName { get; init; }
    public string? FullName { get; init; }
    public Role Role { get; set; }

    public override string ToString() => $"{UserName ?? FullName} ({Id})";
}