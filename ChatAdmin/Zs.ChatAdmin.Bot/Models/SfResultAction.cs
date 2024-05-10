using System.Text.Json.Serialization;

namespace ChatAdmin.Bot.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
internal enum SfResultAction
{
    Continue,
    DeleteMessage,
    SetAccountingStartDate,
    SendMessageToGroup,
    SendMessageToOwner
}
