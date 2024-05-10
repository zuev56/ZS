using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zs.Bot.Data.Repositories;
using Zs.Bot.Data.Queries;
using Zs.Bot.Data.Storages;
using Zs.Common.Abstractions;
using Zs.Common.Exceptions;
using Zs.Common.Models;
using static Zs.Bot.Data.SQLite.FaultCodes;

namespace Zs.Bot.Data.SQLite;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqliteMessageDataStorage(this IServiceCollection services, string connectionString)
    {
        var serviceProvider = services.BuildServiceProvider();

        services.AddDbContextFactory<SQLiteBotContext>(options => options.UseSqlite(connectionString));

        var logger = serviceProvider.GetService<ILogger<DbClient>>();
        services.AddSingleton<IDbClient>(new DbClient(connectionString, logger));
        services.AddSingleton<IQueryFactory, QueryFactory>();
        services.AddSingleton<IChatsRepository, ChatsRepository<SQLiteBotContext>>();
        services.AddSingleton<IUsersRepository, UsersRepository<SQLiteBotContext>>();
        services.AddSingleton<IMessagesRepository, MessagesRepository<SQLiteBotContext>>();
        services.AddSingleton<IMessageDataStorage, MessageDataDbStorage>();

        return services;
    }

    public static IServiceCollection AddSqliteMessageDataStorage(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var fault = new Fault(ConnectionStringRequired);
            throw new FaultException(fault);
        }

        return services.AddSqliteMessageDataStorage(connectionString);
    }
}