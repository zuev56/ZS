using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Bot.Data.PostgreSQL;
using Zs.Bot.Data.Repositories;
using Zs.Bot.Services;
using Zs.Bot.Telegram.Extensions;
using Zs.Common.Abstractions;
using Zs.Home.Bot.Interaction;

namespace Zs.Home.Bot;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["ConnectionStrings:Default"];
        services.AddDbContextFactory<PostgreSqlBotContext>(options => options.UseNpgsql(connectionString));

        services.AddSingleton<IChatsRepository, ChatsRepository<PostgreSqlBotContext>>();
        services.AddSingleton<IUsersRepository, UsersRepository<PostgreSqlBotContext>>();
        services.AddSingleton<IMessagesRepository, MessagesRepository<PostgreSqlBotContext>>();

        return services;
    }

    internal static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<BotSettings>()
            .Bind(configuration.GetSection(BotSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var settings = services.BuildServiceProvider().GetRequiredService<IOptions<BotSettings>>().Value;
        services.AddTelegramBotClient(settings.Token);
        services.AddPostgreSqlMessageDataStorage();
        services.AddCommandManager(settings.CliPath);

        return services;
    }

    internal static IServiceCollection AddDbClient(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["ConnectionStrings:Default"]!;

        services.AddSingleton<IDbClient, DbClient>(sp =>
            new DbClient(connectionString, sp.GetService<ILogger<DbClient>>()));

        return services;
    }

    internal static IServiceCollection AddInteractionServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<NotifierSettings>()
            .Bind(configuration.GetSection(NotifierSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<Notifier>();
        services.AddSingleton<SystemStatusService>();
        services.AddSingleton<CommandHandler>();

        return services;
    }

}
