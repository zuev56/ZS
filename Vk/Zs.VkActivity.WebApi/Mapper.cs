﻿using System;
using System.Collections.Generic;
using System.Linq;
using Zs.Common.Extensions;
using Zs.VkActivity.Data.Models;
using Zs.VkActivity.WebApi.Models;

namespace Zs.VkActivity.WebApi;

public static class Mapper
{
    public static ListUserDto ToListUserDto(ActivityListItem activityListItem)
    {
        ArgumentNullException.ThrowIfNull(nameof(activityListItem));

        return new ListUserDto
        {
            Id = activityListItem.User!.Id,
            Name = $"{activityListItem.User!.FirstName} {activityListItem.User.LastName}",
            IsOnline = activityListItem.IsOnline,
            ActivitySec = activityListItem.ActivitySec,
        };
    }

    public static UserDto ToDto(this User user)
    {
        ArgumentNullException.ThrowIfNull(nameof(user));

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Status = user.Status.ToString()
        };
    }

    public static PeriodInfoDto ToPeriodInfoDto(DetailedActivity detailedActivity)
    {
        ArgumentNullException.ThrowIfNull(nameof(detailedActivity));

        return new PeriodInfoDto
        {
            UserId = detailedActivity.UserId,
            UserName = detailedActivity.UserName,
            VisitInfos = detailedActivity.VisitInfos.ToDtos(),
            AllVisitsCount = detailedActivity.AllVisitsCount,
            FullTime = detailedActivity.FullTime.ToDayHHmmss(),
            AvgDailyTime = detailedActivity.AvgDailyTime.ToDayHHmmss(),
            ActivityDaysCount = detailedActivity.ActivityDaysCount,
            AnalyzedDaysCount = detailedActivity.AnalyzedDaysCount
        };
    }

    private static List<VisitInfoDto> ToDtos(this List<VisitInfo> visitInfos)
        => visitInfos.Select(vi => new VisitInfoDto
        {
            Platform = vi.Platform.ToString(),
            Count = vi.Count,
            Time = vi.Time.ToDayHHmmss()
        })
        .ToList();
}
