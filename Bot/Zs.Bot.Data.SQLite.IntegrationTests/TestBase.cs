using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zs.Bot.Data.Models;
using Zs.Bot.Data.Queries;
using Zs.Bot.Data.Repositories;
using Zs.Common.Abstractions;

namespace Zs.Bot.Data.SQLite.IntegrationTests;

public abstract class TestBase : IDisposable
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly IFixture Fixture = new Fixture();
    protected readonly string ConnectionString = $"Data Source=./IntegrationTestDatabase_{Guid.NewGuid()}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-zzz}.db;";

    public static RawDataStructure RawDataStructure => CreateRawDataStructure();

    protected TestBase()
    {
        ServiceProvider = CreateServiceProvider();
        InitializeDataBase();
    }

    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContextFactory<SQLiteBotContext>(options => options.UseSqlite(ConnectionString));

        services.AddSingleton(RawDataStructure);
        services.AddSingleton<IDbClient>(new DbClient(ConnectionString));
        services.AddSingleton<IQueryFactory, QueryFactory>();
        services.AddSingleton<IChatsRepository, ChatsRepository<SQLiteBotContext>>();
        services.AddSingleton<IUsersRepository, UsersRepository<SQLiteBotContext>>();
        services.AddSingleton<IMessagesRepository, MessagesRepository<SQLiteBotContext>>();

        return services.BuildServiceProvider();
    }

    private void InitializeDataBase()
    {
        var dbContextFactory = ServiceProvider.GetRequiredService<IDbContextFactory<SQLiteBotContext>>();
        using var context = dbContextFactory.CreateDbContext();
        context.Database.EnsureCreated();
    }

    private static RawDataStructure CreateRawDataStructure()
    {
        return new RawDataStructure
        {
            Chat = new RawChatPaths
            {
                Id = "$.Id",
                Name = "$.Username"
            },
            User = new RawUserPaths
            {
                Id = "$.Id",
                Name = "$.Username",
                IsBot = "$.IsBot"
            },
            Message = new RawMessagePaths
            {
                Id = "$.MessageId",
                Text = "$.Text",
                ChatId = "$.Chat.Id",
                UserId = "$.From.Id",
                Date = "$.Date"
            }
        };
    }

    protected async Task FillDatabaseWithTestDataSetAsync(int itemsAmount = 100)
    {
        var chats = Enumerable.Range(0, itemsAmount)
            .Select(id => Fixture.Build<Chat>()
                .With(c => c.RawData, CreateRawDataForChat(id))
                .Create())
            .ToArray();
        var chatsRepository = ServiceProvider.GetRequiredService<IChatsRepository>();
        await chatsRepository.AddRangeAsync(chats);

        var users = Enumerable.Range(0, itemsAmount)
            .Select(id => Fixture.Build<User>()
                .With(u => u.RawData, CreateRawDataForUser(id))
                .With(u => u.Role)
                .Create())
            .ToArray();
        var usersRepository = ServiceProvider.GetRequiredService<IUsersRepository>();
        await usersRepository.AddRangeAsync(users);

        var messages = Enumerable.Range(0, itemsAmount)
            .Select(id =>
            {
                var rawChatId = chats.GetRandomValue().Id;
                var rawUserId = users.GetRandomValue().Id;
                return Fixture.Build<Message>()
                    .OmitAutoProperties()
                    .With(m => m.ChatId, rawChatId)
                    .With(m => m.UserId, rawUserId)
                    .With(m => m.RawData, CreateRawDataForMessage(id, rawChatId, rawUserId))
                    .Create();
            })
            .ToArray();
        var messagesRepository = ServiceProvider.GetRequiredService<IMessagesRepository>();
        await messagesRepository.AddRangeAsync(messages);

    }

    private string CreateRawDataForChat(int rawChatId)
    {
        var firstName = Fixture.Create<string>();
        var lastName = Fixture.Create<string>();
        var userName = Fixture.Create<string>();

        return $"{{\"Id\":{rawChatId},\"Type\":1,\"Username\":\"{userName}\",\"FirstName\":\"{firstName}\",\"LastName\":\"{lastName}\"}}";
    }

    private string CreateRawDataForUser(int rawUserId)
    {
        var firstName = Fixture.Create<string>();
        var lastName = Fixture.Create<string>();
        var userName = Fixture.Create<string>();

        return $"{{\"Id\":{rawUserId},\"IsBot\":false,\"FirstName\":\"{firstName}\",\"LastName\":\"{lastName}\",\"Username\":\"{userName}\",\"LanguageCode\":\"en\"}}";
    }

    private string CreateRawDataForMessage(int rawMessageId, long rawChatId, long rawUserId)
    {
        var firstName = Fixture.Create<string>();
        var lastName = Fixture.Create<string>();
        var userName = Fixture.Create<string>();
        var messageText = Fixture.Create<string>();

        return $"{{\"MessageId\":{rawMessageId}," +
                      $"\"From\":{{\"Id\":{rawUserId},\"IsBot\":false,\"FirstName\":\"{firstName}\",\"LastName\":\"{lastName}\",\"Username\":\"{userName}\",\"LanguageCode\":\"en\"}}," +
                      $"\"Date\":\"2023-07-24T06:28:02Z\"," +
                      $"\"Chat\":{{\"Id\":{rawChatId},\"Type\":1,\"Username\":\"{userName}\",\"FirstName\":\"{firstName}\",\"LastName\":\"{lastName}\"}}," +
                      $"\"Text\":\"{messageText}\",\"Type\":1}}";
    }

    public void Dispose()
    {
        var dbContextFactory = ServiceProvider.GetRequiredService<IDbContextFactory<SQLiteBotContext>>();
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Database.EnsureDeleted();
    }
}