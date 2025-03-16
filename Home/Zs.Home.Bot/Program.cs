using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Zs.Bot.Data.PostgreSQL;
using Zs.Common.Data.Postgres.Services;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Connection;
using Zs.Common.Services.Scheduling;
using Zs.Home.Application.Features.Hardware;
using Zs.Home.Application.Features.Ping;
using Zs.Home.Application.Features.Seq;
using Zs.Home.Application.Features.VkUsers;
using Zs.Home.Application.Features.Weather;
using Zs.Home.Application.Models;
using Zs.Home.Bot.Interaction;

namespace Zs.Home.Bot;

public sealed class Program
{
    public static async Task Main(string[] args)
    {
        var hostBuilder = CreateHostBuilder(args);
        var host = hostBuilder.UseConsoleLifetime().Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogProgramStartup();

        await InitializeDataBaseAsync(host.Services);
        await host.RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");

        return Host.CreateDefaultBuilder(args)
            .ConfigureExternalAppConfiguration(args, Assembly.GetAssembly(typeof(Program))!)
            .ConfigureServices(static (hostContext, services) =>
            {
                var configuration = hostContext.Configuration;

                services
                    .AddDatabase(configuration)
                    .AddConnectionAnalyzer()
                    .AddTelegramBot(configuration)
                    .AddSeq(configuration)
                    .AddDbClient(configuration)
                    .AddWeatherAnalyzer(configuration)
                    .AddUserWatcher(configuration) // TODO: Переедет в Jobs
                    .AddLinuxHardwareMonitor(configuration)
                    .AddInteractionServices(configuration)
                    .AddPingChecker(configuration)
                    .AddSingleton<IScheduler, Scheduler>()
                    .AddSerilog(loggerConfig => loggerConfig.ReadFrom.Configuration(configuration))
                    .AddHostedService<HomeBot>();
            })
            .ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder.Configure(app =>
                {
                    app.UseRouting();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/healthcheck", async (HttpContext context) =>
                        {
                            var connectionString = context.RequestServices
                                .GetRequiredService<IConfiguration>()
                                .GetConnectionString("Default")!;
                            var currentProcess = Process.GetCurrentProcess();
                            var dbTables = await DbInfoService.GetInfoAsync(connectionString, "bot");
                            var healthStatus = HealthStatus.Get(currentProcess, dbTables);

                            return Results.Ok(healthStatus);
                        });

                        endpoints.MapPost("/send", async (Notification notification, Notifier notifier) =>
                        {
                            await notifier.NotifyAsync(notification.Text);
                            return Results.Ok();
                        });
                    });
                });
            });
    }

    private static async Task InitializeDataBaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<PostgreSqlBotContext>();

        await db.Database.EnsureCreatedAsync();
    }
}
