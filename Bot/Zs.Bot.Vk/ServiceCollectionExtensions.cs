using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
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
        vkApi.VkApiVersion.SetVersion(5, 199);
        vkApi.Authorize(new ApiAuthParams { AccessToken = token, Settings = Settings.All});
        //var onlineStatus = vkApi.Groups.GetOnlineStatus(group_id);
        //var members = vkApi.Groups.GetMembers(new GroupsGetMembersParams {GroupId = "zuev56_bot"});
        //var messageSendingExample = vkApi.Messages.Send(new MessagesSendParams {UserId = 8790237, Message = "Hello", RandomId = Random.Shared.NextInt64(long.MaxValue)});
        //var server = vkApi.Groups.GetLongPollServer(group_id);
        //var settings = vkApi.Groups.GetLongPollSettings(group_id);
        //var history = vkApi.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams { Server = server.Server, Key = server.Key, Ts = server.Ts });
        // var server = vkApi.Groups.GetLongPollServer(group_id);
        // var settings = vkApi.Groups.GetLongPollSettings(group_id);
        // var history = vkApi.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams { Server = server.Server, Key = server.Key, Ts = server.Ts });

        return services.AddSingleton<IBotClient>(new BotClient(vkApi, botClientLogger));
    }
}
