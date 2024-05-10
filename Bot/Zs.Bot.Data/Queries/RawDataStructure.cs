namespace Zs.Bot.Data.Queries;

public sealed record RawDataStructure
{
    public RawUserPaths User { get; init; } = null!;
    public RawChatPaths Chat { get; init; } = null!;
    public RawMessagePaths Message { get; init; } = null!;
}