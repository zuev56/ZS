using System.ComponentModel.DataAnnotations;

namespace Zs.DemoBot;

internal sealed record Settings
{
    [Required]
    public string BotToken { get; init; } = null!;

    public string? BotName { get; init; }

    [Required]
    public long OwnerChatRawId { get; init; }

    [Required]
    public IReadOnlyList<long> PrivilegedUserRawIds { get; init; } = Array.Empty<long>();

    [Required]
    public string CliPath { get; init; } = null!;
}