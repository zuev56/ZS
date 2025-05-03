namespace Zs.Home.WebApi.Features.Ping.Models;

public sealed record PingResponse(IReadOnlyList<PingResult> PingResults)
{
    public IReadOnlyDictionary<int, string> StatusCodeToNameMap => PingResults
        .Select(p => p.Status)
        .Distinct()
        .ToDictionary(s => (int)s, s => s.ToString());
}
