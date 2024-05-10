namespace Zs.Bot.Data.Models;

public sealed class Message : DbEntity
{
    public long? ReplyToMessageId { get; set; }
    public long UserId { get; set; }
    public long ChatId { get; set; }
    public bool IsDeleted { get; set; }

    public Message? ReplyToMessage { get; set; }
    public User User { get; set; } = null!;
    public Chat Chat { get; set; } = null!;

    public override string ToString() => $"{Id}";
}