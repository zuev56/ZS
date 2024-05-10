using System;
using System.Text.Json.Serialization;
using Zs.Bot.Data.Abstractions;

namespace ChatAdmin.Bot.Models;

internal sealed partial class Accounting : IDbEntity<Accounting, int>
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime UpdateDate { get; set; }

    [JsonIgnore]
    public Func<Accounting> GetItemForSave => () => this;
    [JsonIgnore]
    public Func<Accounting, Accounting> GetItemForUpdate => (existingItem) => this;
}

