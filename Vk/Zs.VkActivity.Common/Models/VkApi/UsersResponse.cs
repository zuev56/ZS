using System.Text.Json.Serialization;

namespace Zs.VkActivity.Common.Models.VkApi;

public sealed class UsersResponse : VkApiResponse
{
    [JsonPropertyName("response")]
    public List<UserResponse>? Users { get; init; }
}
