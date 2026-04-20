using DbUp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace Zs.Home.AIAgent.Worker.Plugins.Music;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMusicPlugin(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MusicPluginSettings>()
            .Bind(configuration.GetSection(MusicPluginSettings.SectionName))
            .ValidateOnStart();

        services.AddSingleton<Repository>();
        services.AddSingleton<MusicPlugin>();

        var serviceProvider = services.BuildServiceProvider();

        var connectionString = serviceProvider.GetRequiredService<IOptions<MusicPluginSettings>>().Value.ConnectionString;
        UpdateDatabase(connectionString);

        var musicPlugin = serviceProvider.GetRequiredService<MusicPlugin>();

        var kernel = serviceProvider.GetRequiredService<Kernel>();
        kernel.Plugins.AddFromObject(musicPlugin, MusicPluginSettings.PluginName);

        return services;
    }

    private static void UpdateDatabase(string connectionString)
    {
        var upgradeResult = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsFromFileSystem("./Plugins/Music/Migrations")
            .JournalToPostgresqlTable("public", "music_schema_versions")
            .Build()
            .PerformUpgrade();

        if (!upgradeResult.Successful)
            throw new Exception("Database migration failed!", upgradeResult.Error);
    }
}
