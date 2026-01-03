using System.Text.Json.Serialization;

namespace Zs.VkActivity.Common.Models.VkApi;

public sealed class FriendsApiResponse : VkApiResponseBase
{
    [JsonPropertyName("response")]
    public ResponseData? Data { get; init; }

    public sealed class ResponseData
    {
        [JsonPropertyName("count")]
        public int Count { get; init; }

        [JsonPropertyName("items")]
        public int[]? FriendIds { get; init; }
    }
}
