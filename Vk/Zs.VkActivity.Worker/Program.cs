using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Zs.Common.Services.Connection;
using Zs.Common.Services.Logging.DelayedLogger;
using Zs.Common.Services.Scheduling;
using Zs.VkActivity.Common;
using Zs.VkActivity.Common.Abstractions;
using Zs.VkActivity.Common.Services;
using Zs.VkActivity.Data;
using Zs.VkActivity.Data.Abstractions;
using Zs.VkActivity.Data.Repositories;
using Zs.VkActivity.Worker.Abstractions;
using Zs.VkActivity.Worker.Services;

[assembly: InternalsVisibleTo("Worker.UnitTests")]
[assembly: InternalsVisibleTo("Worker.IntegrationTests")]


var host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices(ConfigureServices)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(host.Services.GetService<IConfiguration>())
    .CreateLogger();

Log.Warning("-! Starting {ProcessName} (MachineName: {MachineName}, OS: {OS}, User: {User}, ProcessId: {ProcessId})",
    Process.GetCurrentProcess().MainModule?.ModuleName, Environment.MachineName,
    Environment.OSVersion, Environment.UserName, Environment.ProcessId);

await host.RunAsync();


void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddDbContext<VkActivityContext>(options =>
        options.UseNpgsql(context.Configuration.GetConnectionString(AppSettings.ConnectionStrings.Default)));

    services.AddScoped<IDbContextFactory<VkActivityContext>, VkActivityContextFactory>();

    services.AddConnectionAnalyzer();
    services.AddVkIntegration(context.Configuration);
    services.AddSingleton<IScheduler, Scheduler>();
    // TODO: Create Factory!
    services.AddSingleton<IDelayedLogger<ActivityLogger>, DelayedLogger<ActivityLogger>>();
    services.AddSingleton<IDelayedLogger<WorkerService>, DelayedLogger<WorkerService>>();

    services.AddScoped<IUserManager, UserManager>();
    services.AddScoped<IActivityLogger, ActivityLogger>();

    services.AddScoped<IActivityLogItemsRepository, ActivityLogItemsRepository>();
    services.AddScoped<IUsersRepository, UsersRepository>();

    services.AddHostedService<WorkerService>();
}