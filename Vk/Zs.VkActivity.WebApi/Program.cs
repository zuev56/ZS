using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using Zs.Common.Extensions;
using Zs.VkActivity.Common;
using Zs.VkActivity.Common.Abstractions;
using Zs.VkActivity.Common.Services;
using Zs.VkActivity.Data;
using Zs.VkActivity.Data.Abstractions;
using Zs.VkActivity.Data.Repositories;
using Zs.VkActivity.WebApi;
using Zs.VkActivity.WebApi.Abstractions;
using Zs.VkActivity.WebApi.Services;
using static Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders;

[assembly: InternalsVisibleTo("Api.UnitTests")]
[assembly: InternalsVisibleTo("Api.IntegrationTests")]

var host = Host.CreateDefaultBuilder(args)
    .ConfigureExternalAppConfiguration(args, Assembly.GetAssembly(typeof(Program))!)
    .ConfigureWebHostDefaults(ConfigureWebHostDefaults)
    .ConfigureServices(ConfigureServices)
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogProgramStartup();

await host.RunAsync();


static void ConfigureWebHostDefaults(IWebHostBuilder webHostBuilder)
{
    webHostBuilder.ConfigureServices((context, services) =>
    {
        services.AddSwaggerDocument(config =>
        {
            config.PostProcess = document =>
            {
                document.Info.Title = "VkActivity API";
                document.Info.Version = "v1";
            };
        });

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyOrigin()
                      .WithMethods("HEAD", "GET", "POST", "PUT", "PATCH", "UPDATE", "DELETE")
                      .AllowAnyHeader()
            );
        });

        services.AddControllers()
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                //o.JsonSerializerOptions.Converters.Add(new JsonBooleanConverter()); Can't setup in tests
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options
           => options.SwaggerDoc(context.Configuration[AppSettings.Swagger.ApiVersion],
            new OpenApiInfo
            {
                Title = context.Configuration[AppSettings.Swagger.ApiTitle],
                Version = context.Configuration[AppSettings.Swagger.ApiVersion]
            })
        );

        services.Configure<RouteOptions>(o => o.LowercaseUrls = true);
        services.Configure<ForwardedHeadersOptions>(o => o.ForwardedHeaders = XForwardedFor | XForwardedProto);
    })
    .Configure((context, app) =>
    {
        app.UseSerilogRequestLogging(
            opts => opts.EnrichDiagnosticContext = LogHelper.EnrichFromRequest);

        // Configure the HTTP request pipeline.
        if (!context.HostingEnvironment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseForwardedHeaders();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint(
            context.Configuration[AppSettings.Swagger.EndpointUrl],
            context.Configuration[AppSettings.Swagger.ApiTitle] + " " + context.Configuration[AppSettings.Swagger.ApiVersion])
        );
        app.UseOpenApi();

        app.UseRouting();

        app.UseCors();
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "Default",
                pattern: "api/{controller}/{action}/{id?}");

            endpoints.MapControllers();
        });
    })
    .ConfigureKestrel((_, serverOptions) =>
    {
        // https://docs.microsoft.com/ru-ru/aspnet/core/fundamentals/servers/kestrel/options?view=aspnetcore-6.0

        serverOptions.Limits.MaxConcurrentConnections = 100;
        serverOptions.Limits.MaxConcurrentUpgradedConnections = 100;
        serverOptions.Limits.MaxRequestBodySize = 10 * 1024;
        serverOptions.Limits.MinRequestBodyDataRate =
            new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
        serverOptions.Limits.MinResponseDataRate =
            new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
        serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
        serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);

        //serverOptions.Listen(IPAddress.Loopback, 5010);
        //serverOptions.Listen(IPAddress.Loopback, 5001,
        //    listenOptions =>
        //    {
        //        listenOptions.UseHttps("testCert.pfx",
        //            "testPassword");
        //    });
    });
}

static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    var configuration = context.Configuration;

    services.AddDbContext<VkActivityContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString(AppSettings.ConnectionStrings.Default)));

    services.AddScoped<IDbContextFactory<VkActivityContext>, VkActivityContextFactory>();

    services.AddScoped<ApiExceptionFilter>();

    services.AddVkIntegration(configuration);

    services.AddScoped<IUserManager, UserManager>();
    services.AddScoped<IActivityAnalyzer, ActivityAnalyzer>();

    services.AddScoped<IActivityLogItemsRepository, ActivityLogItemsRepository>();
    services.AddScoped<IUsersRepository, UsersRepository>();

    services.AddSerilog();
}
