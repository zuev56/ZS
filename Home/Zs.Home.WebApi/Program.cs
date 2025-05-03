using System.Reflection;
using Zs.Home.Application.Features.Hardware;
using Zs.Home.Application.Features.Seq;
using Zs.Home.Application.Features.Weather;
using Zs.Home.WebApi;
using Zs.Home.WebApi.Features.Ping;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.EnableTryItOutByDefault();
    options.DisplayRequestDuration();
    options.SwaggerEndpoint(
        builder.Configuration[SwaggerSettings.EndpointUrl],
        builder.Configuration[SwaggerSettings.ApiTitle] + " " + builder.Configuration[SwaggerSettings.ApiVersion]);
});
app.UseOpenApi();
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
