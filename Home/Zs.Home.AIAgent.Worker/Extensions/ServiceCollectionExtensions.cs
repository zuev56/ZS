using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Tavily;
using Zs.Home.AIAgent.Worker.Models;

namespace Zs.Home.AIAgent.Worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSemanticKernel(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var settings = serviceProvider.GetRequiredService<IOptions<OpenAiSettings>>().Value;

        #pragma warning disable SKEXP0050
        var kernelBuilder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(settings.ModelName, new Uri(settings.Url), settings.ApiKey);

        if (!string.IsNullOrWhiteSpace(settings.TavilyTextSearchApiKey))
            kernelBuilder.AddTavilyTextSearch(settings.TavilyTextSearchApiKey);

        var kernel = kernelBuilder.Build();

        if (!string.IsNullOrWhiteSpace(settings.TavilyTextSearchApiKey))
        {
            var textSearch = new TavilyTextSearch(settings.TavilyTextSearchApiKey);
            kernel.Plugins.Add(textSearch.CreateWithSearch("TavilySearch"));
        }

        services.AddSingleton(kernel);

        return services;
    }
}
