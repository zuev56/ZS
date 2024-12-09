using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Zs.Common.Extensions;
using Zs.Home.Application.Features.Weather.Data;
using Zs.Home.Application.Features.Weather.Data.Models;
using Zs.Home.Jobs.Hangfire;
using Zs.Home.Jobs.Hangfire.Hangfire;
using Zs.Home.Jobs.Hangfire.WeatherRegistrator;
using Place = Zs.Home.Application.Features.Weather.Data.Models.Place;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHangfire(builder.Configuration)
    .AddWeatherRegistrator(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

var app = builder.Build();

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


var jobSettings = app.Services.GetRequiredService<IOptions<WeatherRegistratorSettings>>().Value;
RecurringJob.AddOrUpdate<WeatherRegistratorJob>(
    nameof(WeatherRegistratorJob), job => job.ExequteAsync(CancellationToken.None), jobSettings.CronExpression);

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
