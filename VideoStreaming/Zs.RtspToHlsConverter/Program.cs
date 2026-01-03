using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Zs.Common.Extensions;
using Zs.RtspToHlsConverter;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureExternalAppConfiguration(args, Assembly.GetAssembly(typeof(Program))!)
    .UseSerilog()
    .ConfigureServices(services => services.AddHostedService<Worker>())
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(host.Services.GetRequiredService<IConfiguration>())
    .CreateLogger();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogProgramStartup();

await host.RunAsync();
