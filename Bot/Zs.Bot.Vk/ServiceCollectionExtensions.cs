using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VkNet;
using VkNet.Model;
// using VkNet.Model.RequestParams.Groups;
using Zs.Bot.Services.Messaging;

namespace Zs.Bot.Vk;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVkBotClient(this IServiceCollection services, string token)
    {
        var serviceProvider = services.BuildServiceProvider();
        var vkApiLogger = serviceProvider.GetRequiredService<ILogger<VkApi>>();
        var botClientLogger = serviceProvider.GetRequiredService<ILogger<BotClient>>();

        var vkApi = new VkApi(vkApiLogger);
        // vkApi.Groups.
        vkApi.Authorize(new ApiAuthParams { AccessToken = token, ApplicationId = 7572604});

        return services.AddSingleton<IBotClient>(new BotClient(vkApi, botClientLogger));
    }
}
