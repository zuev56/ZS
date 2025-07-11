using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;
using Zs.Home.Application.Features.VkUsers;
using Zs.Home.Application.Features.Weather.Data;
using Zs.Home.ClientApp.Data;
using Zs.Home.ClientApp.Pages.Dashboard.Weather;
using Zs.Home.WebApi.Client.Bootstrap;

namespace Zs.Home.ClientApp;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.ConfigureExternalAppConfiguration(args, Assembly.GetAssembly(typeof(Program))!);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("Identity") ??
                               throw new InvalidOperationException("Connection string 'Identity' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services
            .AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        var homeConnectionString = builder.Configuration.GetConnectionString("Home") ??
                               throw new InvalidOperationException("Connection string 'Home' not found.");
        builder.Services.AddDbContextFactory<WeatherRegistratorDbContext>(
            options => options.UseNpgsql(homeConnectionString));

        builder.Services.AddOptions<WeatherDashboardSettings>()
            .Bind(builder.Configuration.GetSection(WeatherDashboardSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services
            .AddHomeClient(builder.Configuration)
            .AddUserWatcher(builder.Configuration);

        builder.Services
            .AddMediatR(config=> config.RegisterServicesFromAssemblies(typeof(Program).Assembly))
            .AddRazorPages();

        builder.Logging.AddConsole();

        var app = builder.Build();

        app.Logger.LogProgramStartup();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStaticFiles();

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        //app.MapStaticAssets();
        app.MapRazorPages();
            //.WithStaticAssets();

        app.Run();
    }
}
