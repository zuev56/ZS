using System;
using System.Text.Json.Serialization;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Models;

namespace ChatAdmin.Bot.Models;

/// <summary>Информация о банах</summary>
internal sealed partial class Ban : IDbEntity<Ban, int>
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ChatId { get; set; }
    public int? WarningMessageId { get; set; }
    public DateTime? FinishDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public DateTime InsertDate { get; set; }

    [JsonIgnore]
    public Func<Ban> GetItemForSave => () => this;
    [JsonIgnore]
    public Func<Ban, Ban> GetItemForUpdate => (existingItem) => this;

    public User User { get; set; }
    public Chat Chat { get; set; }
    public Message WarningMessage { get; set; }
}

