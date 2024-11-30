using System;

namespace Zs.Home.Jobs.Hangfire.Hangfire;

public sealed class Lock
{
    public string Resource { get; set; } = null!;

    public int Updatecount { get; set; }

    public DateTime? Acquired { get; set; }
}
