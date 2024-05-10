﻿using Zs.VkActivity.Data.Models;

namespace Zs.VkActivity.WebApi.Models;

/// <summary>
/// Used to show users list with their status and activity time
/// </summary>
public sealed class ActivityListItem
{
    public User User { get; }
    public int ActivitySec { get; init; }
    public bool IsOnline { get; init; }

    public ActivityListItem(User user)
    {
        User = user;
    }
}