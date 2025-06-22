using System.Reflection;
using Serilog;
using Zs.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureExternalAppConfiguration(args, Assembly.GetAssembly(typeof(Program))!);
builder.Host.UseSerilog();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

app.Logger.LogProgramStartup();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
