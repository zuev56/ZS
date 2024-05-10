using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zs.VkActivity.Common.Abstractions;
using Zs.VkActivity.Common.Services;

namespace Zs.VkActivity.Common;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVkIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddSingleton<IVkIntegration, VkIntegration>(
            sp => new VkIntegration(
                configuration[AppSettings.Vk.AccessToken]!,
                configuration[AppSettings.Vk.Version]!)
            );
    }
}