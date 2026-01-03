using System.Text.Json.Serialization;

namespace Zs.VkActivity.Common.Models.VkApi;

public sealed class UsersApiResponse : VkApiResponseBase
{
    [JsonPropertyName("response")]
    public List<VkApiUser>? Users { get; init; }
}
