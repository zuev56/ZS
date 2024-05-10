namespace Zs.Bot.Data.Queries;

public record RawMessagePaths
{
    public string Id { get; init; } = null!;
    public string Text { get; init; } = null!;
    public string ChatId { get; init; } = null!;
    public string UserId { get; init; } = null!;
    public string Date { get; init; } = null!;

    public void Deconstruct(out string id, out string text, out string chatId, out string userId, out string date)
    {
        id = Id;
        text = Text;
        chatId = ChatId;
        userId = UserId;
        date = Date;
    }
}