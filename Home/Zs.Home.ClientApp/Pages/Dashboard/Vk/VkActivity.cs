using System;
using System.Collections.Generic;

namespace Zs.Home.ClientApp.Pages.Dashboard.Vk;

public sealed record VkActivity
{
    public required IReadOnlyList<User> Users { get; init; }
}

public sealed record User(string Name, TimeSpan InactiveTime, LastActivity LastActivity);

public enum LastActivity { Recent, LongAgo, TooLongAgo }
