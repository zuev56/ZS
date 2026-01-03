using System.Reflection;
using Serilog;
using Zs.Common.Extensions;
using Zs.Home.Application.Features.Hardware;
using Zs.Home.Application.Features.Seq;
using Zs.Home.WebApi;
using Zs.Home.WebApi.Features.Ping;
using Zs.Home.WebApi.Features.Weather;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.ConfigureExternalAppConfiguration(args, Assembly.GetAssembly(typeof(Program))!);
builder.Host.UseSerilog();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddHardwareMonitor(builder.Configuration);
builder.Services.AddSeqLogAnalyzer(builder.Configuration);
builder.Services.AddPingChecker(builder.Configuration);
builder.Services.AddWeatherAnalyzer(builder.Configuration);
builder.Services.AddMediatR(
    config => config.RegisterServicesFromAssemblies(typeof(Program).GetTypeInfo().Assembly));

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerDocument(config =>
{
    config.PostProcess = document =>
    {
        document.Info.Title = builder.Configuration[SwaggerSettings.ApiTitle];
        document.Info.Version = builder.Configuration[SwaggerSettings.ApiVersion];
    };
});

var app = builder.Build();

app.Logger.LogProgramStartup();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.EnableTryItOutByDefault();
    options.DisplayRequestDuration();
    options.SwaggerEndpoint(
        builder.Configuration[SwaggerSettings.EndpointUrl],
        builder.Configuration[SwaggerSettings.ApiTitle] + " " + builder.Configuration[SwaggerSettings.ApiVersion]);
});
// app.UseOpenApi();
app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


/*
 * Weather // Для вывода информации с устройств из разных сфер выгоднее иметь обобщённый DeviceController
 *      EspMeteoStatus(IP)
 *        -Temperature
 *        -Humidity
 *        -Pressure
 *      Forecast|CurrentOutside

 * Seq
 *      Week
 *      24 hours
 *      12 hours
 *      6 hours
 *      Last hour
 *
 * OS
 *		Journal analyzis
 *
 * Hardware // DeviceController
 *      CPU temperature
 *      CPU usage
 *      Memory usage
 *
 * Services
 *      HealthCheck
 *      Stop
 *      Start
 *      Restart
 *
 * Network
 *      Device list
 *      Unknown devices
 *
 * Ping
 *      IP
 *      IP:Port
 *
 */
