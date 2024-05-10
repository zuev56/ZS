using System;
using System.Text.Json.Serialization;
using Zs.Bot.Data.Abstractions;

namespace ChatAdmin.Bot.Models;

/// <summary>Напоминание о событиях</summary>
internal sealed partial class Notification : IDbEntity<Notification, int>
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
    public string Message { get; set; }
    public int? Month { get; set; }
    public int Day { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public DateTime? ExecDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public DateTime InsertDate { get; set; }

    [JsonIgnore]
    public Func<Notification> GetItemForSave => () => this;
    [JsonIgnore]
    public Func<Notification, Notification> GetItemForUpdate => (existingItem) => this;
}
