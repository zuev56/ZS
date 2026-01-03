using System.Text.Json.Serialization;
using Zs.VkActivity.Data.Models;

namespace Zs.VkActivity.Common.Models.VkApi;

// https://dev.vk.ru/reference/objects/user#last_seen
public sealed class LastSeenResponse : VkApiResponseBase
{
    [JsonPropertyName("time")]
    public int UnixTime { get; init; }

    [JsonPropertyName("platform")]
    public Platform Platform { get; init; }
}
