using System.Text.Json.Serialization;

namespace Zs.VkActivity.Common.Models.VkApi;

public abstract class VkApiResponseBase
{
    [JsonPropertyName("error")]
    public Error? Error { get; set; }
}

public sealed class Error
{
    [JsonPropertyName("error_code")]
    public int? ErrorCode { get; set; }

    [JsonPropertyName("error_msg")]
    public string? ErrorMsg { get; set; }

    [JsonPropertyName("request_params")]
    public List<RequestParam> RequestParams { get; } = [];
}

public sealed class RequestParam
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
