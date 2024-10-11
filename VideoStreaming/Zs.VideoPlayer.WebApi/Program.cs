using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zs.VideoPlayer.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services
    // TODO: Вынести зависимость от ffmpeg из API
    //.AddVideoStreamClient()
    //.AddRtspImageService()
    .AddVideoFilesProvider();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(options =>
{
    var origins = app.Configuration.GetSection("CorsPolicy:Origins").Get<string[]>();
    options
        .WithMethods("GET")
        .WithOrigins(origins);
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
