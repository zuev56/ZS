using System;

namespace Zs.Bot.Data.Models;

public abstract class DbEntity
{
    public long Id { get; set; }
    public string RawData { get; set; } = null!;
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}