using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zs.Bot.Data.Queries;
using Zs.Bot.Data.Repositories;
using Zs.Bot.Data.Storages;
using Zs.Common.Abstractions;
using Zs.Common.Exceptions;
using Zs.Common.Models;
using static Zs.Bot.Data.PostgreSQL.FaultCodes;

namespace Zs.Bot.Data.PostgreSQL;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgreSqlMessageDataStorage(this IServiceCollection services, string connectionString)
    {
        var serviceProvider = services.BuildServiceProvider();

        services.AddDbContextFactory<PostgreSqlBotContext>(options => options.UseNpgsql(connectionString));

        var logger = serviceProvider.GetService<ILogger<DbClient>>();
        services.AddSingleton<IDbClient>(new DbClient(connectionString, logger));
        services.AddSingleton<IQueryFactory, QueryFactory>();
        services.AddSingleton<IChatsRepository, ChatsRepository<PostgreSqlBotContext>>();
        services.AddSingleton<IUsersRepository, UsersRepository<PostgreSqlBotContext>>();
        services.AddSingleton<IMessagesRepository, MessagesRepository<PostgreSqlBotContext>>();
        services.AddSingleton<IMessageDataStorage, MessageDataDbStorage>();

        return services;
    }

    public static IServiceCollection AddPostgreSqlMessageDataStorage(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var fault = new Fault(ConnectionStringRequired);
            throw new FaultException(fault);
        }

        return services.AddPostgreSqlMessageDataStorage(connectionString);
    }
}