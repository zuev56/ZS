﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zs.VkActivity.WebApi.Abstractions;
using Zs.VkActivity.WebApi.Models;

namespace Zs.VkActivity.WebApi.Controllers;

// + Накапливать ошибки соединения и выдавать их один раз в 10 минут
// + Анализировать соединение и останавливать запросы к Vk API при разрыве связи
//+/- Раз в день обновлять пользователей в БД (ТЕСТ)
//+/- ActivityAnalyzer.GetActivitySeconds учитывать каждую платформу по отдельности (ТЕСТ)
//+/- Сделать другие контроллеры, необходимые для фронтенда
//+/- Настроить использование appsettings.Development.json
//+/- Сделать настройку Kestrel из конфигурационного файла(возможность переопределения заданных в коде параметров)
// - Покрыть тестами оставшееся!
// - Проверить корректность работы логгера активности пользователей
// - Сделать обработку деактивированного пользователя deactivated: string [ deleted, banned ]
// - Ошибки логировать только на верхнем уровне, а не на каждом этапе
// - Настроить WithOrigins

[Route("api/activity")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[ApiController]
public sealed class ActivityLogController : Controller
{
    private readonly IActivityAnalyzer _activityAnalyzerService;

    public ActivityLogController(IActivityAnalyzer activityAnalyzerService)
    {
        _activityAnalyzerService = activityAnalyzerService;
    }

    [HttpGet("{userId:int}/period/{fromDate:datetime}/{toDate:datetime}")]
    [ProducesResponseType(typeof(PeriodInfoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPeriodInfo(
        [FromRoute] int userId, [FromRoute] DateTime fromDate, [FromRoute] DateTime toDate)
    {
        var periodStatisticsResult = await _activityAnalyzerService.GetUserStatisticsForPeriodAsync(userId, fromDate, toDate);
        var periodInfoDto = Mapper.ToPeriodInfoDto(periodStatisticsResult.Value);

        return Ok(periodInfoDto);
    }

    [HttpGet("{userId:int}/day/{date:datetime}")]
    [ProducesResponseType(typeof(PeriodInfoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDayInfo([FromRoute] int userId, [FromRoute] DateTime date)
    {
        var fromDate = date.Date;
        var toDate = fromDate.AddDays(1).AddMilliseconds(-1);
        var periodStatisticsResult = await _activityAnalyzerService.GetUserStatisticsForPeriodAsync(userId, fromDate, toDate);
        var periodInfoDto = Mapper.ToPeriodInfoDto(periodStatisticsResult.Value);

        return Ok(periodInfoDto);
    }

    [HttpGet("{userId:int}/fulltime")]
    [ProducesResponseType(typeof(PeriodInfoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFullTimeInfo([FromRoute] int userId)
    {
        var tmpMinLogDate = new DateTime(2022, 09, 18);
        var fullTimeStatisticsResult = await _activityAnalyzerService
            .GetUserStatisticsForPeriodAsync(userId, tmpMinLogDate, DateTime.UtcNow);
        var fullTimeInfoDto = Mapper.ToPeriodInfoDto(fullTimeStatisticsResult.Value);

        return Ok(fullTimeInfoDto);
    }

    /// <summary>UTC</summary>
    [HttpGet("{userId:int}/last-utc")]
    [ProducesResponseType(typeof(DateTime), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLastVisitDate([FromRoute] int userId)
    {
        var lastVisitDateResult = await _activityAnalyzerService.GetLastVisitDate(userId);

        return Ok(lastVisitDateResult.Value);
    }

    /// <summary>UTC</summary>
    [HttpGet("{userId:int}/is-online")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> IsOnline([FromRoute] int userId)
    {
        var isOnlineResult = await _activityAnalyzerService.IsOnline(userId);

        return Ok(isOnlineResult.Value);
    }

    //[Obsolete("Use UsersController.GetUsersWithActivity")]
    //[HttpGet(nameof(GetUsersWithActivity))]
    //public async Task<IActionResult> GetUsersWithActivity(string filterText, DateTime fromDate, DateTime toDate)
    //{
    //    var usersWithActivityResult = await _activityAnalyzerService.GetUsersWithActivityAsync(filterText, fromDate, toDate);
    //    usersWithActivityResult.AssertResultIsSuccessful();

    //    return Ok(_mapper.Map<List<ListUserDto>>(usersWithActivityResult.Value));
    //}

    //[HttpGet(nameof(GetUsersWithActivity))]
    //public async Task<IActionResult> GetUsersWithActivity(TableParameters requestParameters)
    //{
    //    // TODO: In future...
    //    throw new NotImplementedException();
    //    //var usersWithActivityResult = await _vkActivityService.GetUsersWithActivityTable(requestParameters);
    //    //usersWithActivityResult.AssertResultIsSuccessful();
    //    //
    //    //return Ok(_mapper.Map<List<ListUserDto>>(usersWithActivityResult.Value));
    //}

}