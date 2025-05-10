using System.Text.Json.Serialization;

namespace Zs.VkActivity.Common.Models.VkApi;

public abstract class VkApiResponse
{
    [JsonPropertyName("error")]
    public Error? Error { get; init; }
}

public class Error
{
    [JsonPropertyName("error_code")]
    public int Code { get; init; }

    [JsonPropertyName("error_msg")]
    public string Message { get; init; } = null!;
}
