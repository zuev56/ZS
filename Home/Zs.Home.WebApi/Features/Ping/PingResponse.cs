using Zs.Home.Application.Features.Ping;

namespace Zs.Home.WebApi.Features.Ping;

public sealed record PingResponse(IReadOnlyList<PingResult> PingResults)
{
    public IReadOnlyDictionary<int, string> StatusCodeToNameMap => PingResults
        .Select(p => p.Status)
        .Distinct()
        .ToDictionary(s => (int)s, s => s.ToString());
}
