using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zs.VkActivity.WebApi;

namespace Zs.Home.Application.Features.VkUsers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserWatcher(this IServiceCollection services, IConfiguration configuration)
        => AddUserWatcher<UserWatcherSettings>(services, configuration);

    public static IServiceCollection AddUserWatcher<TSettings>(this IServiceCollection services, IConfiguration configuration)
        where TSettings : UserWatcherSettings
    {
        var settingsSection = configuration.GetSection(UserWatcherSettings.SectionName);
        services.AddOptions<TSettings>()
            .Bind(settingsSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var baseUrl = settingsSection.Get<TSettings>()!.VkActivityApiUri;

        services.AddSingleton<IActivityLogClient, ActivityLogClient>(_ => new ActivityLogClient(baseUrl));
        services.AddSingleton<IUsersClient, UsersClient>(_ => new UsersClient(baseUrl));
        services.AddSingleton<IUserWatcher, UserWatcher>();

        return services;
    }
}
