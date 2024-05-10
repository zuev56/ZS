using System;
using System.Text.Json.Serialization;

namespace ChatAdmin.Bot.Models;

/// <summary>Вспомогательные слова - то, что должно быть отсеяно из статистики</summary>
internal sealed partial class AuxiliaryWord
{
    public string Word { get; set; }
    public DateTime InsertDate { get; set; }

    [JsonIgnore]
    public Func<AuxiliaryWord> GetItemToSave => () => this;
    [JsonIgnore]
    public Func<AuxiliaryWord, AuxiliaryWord> GetItemToUpdate => (existingItem) => this;
}

