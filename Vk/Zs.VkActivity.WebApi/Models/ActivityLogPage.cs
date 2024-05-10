using System.Collections.Generic;
using Zs.VkActivity.Data.Models;

namespace Zs.VkActivity.WebApi.Models;

public sealed class ActivityLogPage
{
    public ushort Page { get; set; }
    public List<ActivityLogItem> Items { get; set; } = new();
}
