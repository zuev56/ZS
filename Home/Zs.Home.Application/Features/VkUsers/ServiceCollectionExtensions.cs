using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zs.Home.Application.Features.VkUsers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserWatcher(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<UserWatcherSettings>()
            .Bind(configuration.GetSection(UserWatcherSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IUserWatcher, UserWatcher>();

        return services;
    }
}
