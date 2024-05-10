using System;
using Zs.VkActivity.Data.Models;

namespace Zs.VkActivity.WebApi.Models;

public sealed class VisitInfo
{
    public Platform Platform { get; set; }
    public int Count { get; set; }
    public TimeSpan Time { get; set; }
}