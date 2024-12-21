using System.Collections.Generic;

namespace Zs.Home.ClientApp.Pages.Dashboard;

public sealed record PingResult
{
    public required IReadOnlyList<Target> Targets { get; init; }
}

public sealed record Target(string Name, bool IsReachable);
