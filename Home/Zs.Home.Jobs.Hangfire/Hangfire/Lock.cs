using System;

namespace Zs.Home.Jobs.Hangfire.Hangfire;

public sealed class Lock
{
    public string Resource { get; set; } = null!;

    public int UpdateCount { get; set; }

    public DateTime? Acquired { get; set; }
}
