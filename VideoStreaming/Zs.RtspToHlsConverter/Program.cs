using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;
using Zs.RtspToHlsConverter;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureExternalAppConfiguration(args, Assembly.GetAssembly(typeof(Program))!)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogProgramStartup();

await host.RunAsync();
