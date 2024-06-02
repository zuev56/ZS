namespace Zs.Home.WebApi.Features.Ping.Ping;

public sealed record PingResponse
{
    public int BytesCount { get; init; }
    public int Ttl { get; init; }
    public TimeSpan Time { get; init; }
}
