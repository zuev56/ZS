using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;
using Zs.Common.Extensions;
using Zs.Home.Application.Features.VkUsers;
using Zs.Home.Application.Features.Weather.Data;
using Zs.Home.Application.Features.Weather.Data.Models;
using Zs.Home.Jobs.Hangfire.Extensions;
using Zs.Home.Jobs.Hangfire.Hangfire;
using Zs.Home.Jobs.Hangfire.HardwareAnalyzer;
using Zs.Home.Jobs.Hangfire.LogAnalyzer;
using Zs.Home.Jobs.Hangfire.Notification;
using Zs.Home.Jobs.Hangfire.Ping;
using Zs.Home.Jobs.Hangfire.UserWatcher;
using Zs.Home.Jobs.Hangfire.WeatherAnalyzer;
using Zs.Home.Jobs.Hangfire.WeatherRegistrator;
using Zs.Home.WebApi.Client.Bootstrap;
using PingCheckerSettings = Zs.Home.Jobs.Hangfire.Ping.PingCheckerSettings;
using Place = Zs.Home.Application.Features.Weather.Data.Models.Place;
using UserWatcherSettings = Zs.Home.Jobs.Hangfire.UserWatcher.UserWatcherSettings;
using WeatherAnalyzerSettings = Zs.Home.Jobs.Hangfire.WeatherAnalyzer.WeatherAnalyzerSettings;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureExternalAppConfiguration(args, Assembly.GetAssembly(typeof(Program))!);
builder.Host.UseSerilog();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services
    .AddHangfire(builder.Configuration)
    .AddHomeClient(builder.Configuration)
    .AddSingleton<Notifier>()
    .AddJobConfigurations(builder.Configuration)
    // TODO: Получать настройки из API
    .AddUserWatcher<UserWatcherSettings>(builder.Configuration)
    // TODO: Попробовать объединить с настройками из API и брать их оттуда
    .AddWeatherRegistrator(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

var app = builder.Build();

app.Logger.LogProgramStartup();

await InitializeWeatherRegistratorDatabaseAsync(app.Services);
await DeleteHangfireLocksAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// app.UseAuthorization();

app.UseHangfireDashboard();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = Array.Empty<IDashboardAuthorizationFilter>()
});

var logAnalyzerSettings = app.Services.GetRequiredService<IOptions<LogAnalyzerSettings>>().Value;
RecurringJob.AddOrUpdate<LogAnalyzerJob>(
    nameof(LogAnalyzerJob),
    job => job.ExecuteAsync(CancellationToken.None),
    logAnalyzerSettings.CronExpression);

var hardwareAnalyzerSettings = app.Services.GetRequiredService<IOptions<HardwareAnalyzerSettings>>().Value;
RecurringJob.AddOrUpdate<HardwareAnalyzerJob>(
    nameof(HardwareAnalyzerJob),
    job => job.ExecuteAsync(CancellationToken.None),
    hardwareAnalyzerSettings.CronExpression);

var pingCheckerSettings = app.Services.GetRequiredService<IOptions<PingCheckerSettings>>().Value;
RecurringJob.AddOrUpdate<PingCheckerJob>(
    nameof(PingCheckerJob),
    job => job.ExecuteAsync(CancellationToken.None),
    pingCheckerSettings.CronExpression);

var userWatcherSettings = app.Services.GetRequiredService<IOptions<UserWatcherSettings>>().Value;
RecurringJob.AddOrUpdate<UserWatcherJob>(
    nameof(UserWatcherJob),
    job => job.ExecuteAsync(CancellationToken.None),
    userWatcherSettings.CronExpression);

var weatherAnalyzerSettings = app.Services.GetRequiredService<IOptions<WeatherAnalyzerSettings>>().Value;
RecurringJob.AddOrUpdate<WeatherAnalyzerJob>(
    nameof(WeatherAnalyzerJob),
    job => job.ExecuteAsync(CancellationToken.None),
    weatherAnalyzerSettings.CronExpression);

var weatherRegistratorSettings = app.Services.GetRequiredService<IOptions<WeatherRegistratorSettings>>().Value;
RecurringJob.AddOrUpdate<WeatherRegistratorJob>(
    nameof(WeatherRegistratorJob),
    job => job.ExecuteAsync(CancellationToken.None),
    weatherRegistratorSettings.CronExpression);

// TODO: Нужен джоб-хелсчекер. Если бот будет недоступен, то отправить уведомление на email
//       Этот же джоб будет проверять все остальные сервисы. А сам Hangfire будет проверяться ботом.
//       Метод API должен возвращать Zs.Common.Models.HealthStatus и, например,
//       когда было выполнено последнее действие сервисом (нужно реализовать такой функционал)

app.Run();


static async Task InitializeWeatherRegistratorDatabaseAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var scopedServices = scope.ServiceProvider;
    var dbContext = scopedServices.GetRequiredService<WeatherRegistratorDbContext>();

    await dbContext.Database.MigrateAsync();

    var settings = serviceProvider.GetRequiredService<IOptions<WeatherRegistratorSettings>>().Value;

    var dbPlaceIds = await dbContext.Places.Select(p => p.Id).ToListAsync();
    foreach (var place in settings.Places)
    {
        if (dbPlaceIds.Any(id => id == place.Id))
            continue;

        dbContext.Places.Add(new Place { Id = place.Id, Name = place.Name });
    }

    var dbSourceIds = await dbContext.Sources.Select(s => s.Id).ToListAsync();
    foreach (var sensor in settings.Sensors)
    {
        if (dbSourceIds.Any(id => id == sensor.Id))
            continue;

        dbContext.Sources.Add(new Source { Id = sensor.Id, PlaceId = sensor.PlaceId, Name = sensor.Name });
    }

    var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
    if (dbContext.ChangeTracker.HasChanges())
    {
        logger.LogInformationIfNeed("Tables 'source' and 'place' has been changed according to the configuration: {Changes}.", dbContext.ChangeTracker.ToDebugString());
        await dbContext.SaveChangesAsync();
    }
    else
    {
        logger.LogInformationIfNeed("Places and Sources in the database corresponds to the configuration.");
    }
}

static async Task DeleteHangfireLocksAsync(IServiceProvider serviceProvider)
{
    var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();

    using var scope = serviceProvider.CreateScope();
    var scopedServices = scope.ServiceProvider;
    var dbContext = scopedServices.GetRequiredService<HangfireDbContext>();

    try
    {
        var deleted = await dbContext.Locks.ExecuteDeleteAsync();
        if (deleted > 0)
        {
            logger.LogInformationIfNeed("Hangfire locks have been removed.");
        }
    }
    catch (PostgresException e) when (e.MessageText.Contains("does not exist"))
    {
        logger.LogInformationIfNeed("Hangfire tables have not been created yet.");
    }
}
