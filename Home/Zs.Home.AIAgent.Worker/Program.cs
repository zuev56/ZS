using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zs.Home.AIAgent.Worker;
using Zs.Home.AIAgent.Worker.Extensions;
using Zs.Home.AIAgent.Worker.Models;
using Zs.Home.AIAgent.Worker.Plugins.Music;
using Zs.Home.AIAgent.Worker.Plugins.Weather;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext ,services) =>
    {
        var configuration = hostContext.Configuration;

        services.AddOptions<OpenAiSettings>()
            .Bind(configuration.GetSection(OpenAiSettings.SectionName))
            .ValidateOnStart();

        services
            .AddSemanticKernel()
            .AddMusicPlugin(configuration)
            .AddWeatherPlugin(configuration)
            .AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
