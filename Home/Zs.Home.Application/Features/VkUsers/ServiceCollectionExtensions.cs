using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zs.VkActivity.WebApi;

namespace Zs.Home.Application.Features.VkUsers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserWatcher(this IServiceCollection services, IConfiguration configuration)
    {
        var settingsSection = configuration.GetSection(UserWatcherSettings.SectionName);
        services.AddOptions<UserWatcherSettings>()
            .Bind(settingsSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var baseUrl = settingsSection.Get<UserWatcherSettings>()!.VkActivityApiUri;

        services.AddSingleton<IActivityLogClient, ActivityLogClient>(_ => new ActivityLogClient(baseUrl));
        services.AddSingleton<IUsersClient, UsersClient>(_ => new UsersClient(baseUrl));
        services.AddSingleton<IUserWatcher, UserWatcher>();

        return services;
    }
}
