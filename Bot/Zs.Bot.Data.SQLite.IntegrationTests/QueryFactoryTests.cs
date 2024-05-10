using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zs.Bot.Data.Queries;

namespace Zs.Bot.Data.SQLite.IntegrationTests;

public sealed class QueryFactoryTests : TestBase
{
    [Fact]
    public async Task CreateFindByConditionQuery_RawDataEq_CreateExpectedSql()
    {
        var chatId = Fixture.Create<long>();
        var queryFactory = ServiceProvider.GetRequiredService<IQueryFactory>();
        var chatIdPath = queryFactory.RawDataStructure.Chat.Id;
        var condition = RawData.Eq(chatIdPath, chatId);
        var expectedSql = $"select * from Chats where json_extract(RawData, '{chatIdPath}') = {chatId}";

        var sql = queryFactory.CreateFindByConditionQuery("Chats", condition);

        sql.Should().Be(expectedSql);
    }

    [Fact]
    public async Task CreateFindByConditionQuery_RawDataNe_CreateExpectedSql()
    {
        var chatId = Fixture.Create<long>();
        var queryFactory = ServiceProvider.GetRequiredService<IQueryFactory>();
        var chatIdPath = queryFactory.RawDataStructure.Chat.Id;
        var condition = RawData.Ne(chatIdPath, chatId);
        var expectedSql = $"select * from Chats where json_extract(RawData, '{chatIdPath}') != {chatId}";

        var sql = queryFactory.CreateFindByConditionQuery("Chats", condition);

        sql.Should().Be(expectedSql);
    }

    [Fact]
    public async Task CreateFindByConditionQuery_RawDataGt_CreateExpectedSql()
    {
        var chatId = Fixture.Create<long>();
        var queryFactory = ServiceProvider.GetRequiredService<IQueryFactory>();
        var chatIdPath = queryFactory.RawDataStructure.Chat.Id;
        var condition = RawData.Gt(chatIdPath, chatId);
        var expectedSql = $"select * from Chats where json_extract(RawData, '{chatIdPath}') > {chatId}";

        var sql = queryFactory.CreateFindByConditionQuery("Chats", condition);

        sql.Should().Be(expectedSql);
    }

    [Fact]
    public async Task CreateFindByConditionQuery_RawDataGte_CreateExpectedSql()
    {
        var chatId = Fixture.Create<long>();
        var queryFactory = ServiceProvider.GetRequiredService<IQueryFactory>();
        var chatIdPath = queryFactory.RawDataStructure.Chat.Id;
        var condition = RawData.Gte(chatIdPath, chatId);
        var expectedSql = $"select * from Chats where json_extract(RawData, '{chatIdPath}') >= {chatId}";

        var sql = queryFactory.CreateFindByConditionQuery("Chats", condition);

        sql.Should().Be(expectedSql);
    }

    [Fact]
    public async Task CreateFindByConditionQuery_RawDataLt_CreateExpectedSql()
    {
        var chatId = Fixture.Create<long>();
        var queryFactory = ServiceProvider.GetRequiredService<IQueryFactory>();
        var chatIdPath = queryFactory.RawDataStructure.Chat.Id;
        var condition = RawData.Lt(chatIdPath, chatId);
        var expectedSql = $"select * from Chats where json_extract(RawData, '{chatIdPath}') < {chatId}";

        var sql = queryFactory.CreateFindByConditionQuery("Chats", condition);

        sql.Should().Be(expectedSql);
    }

    [Fact]
    public async Task CreateFindByConditionQuery_RawDataLte_CreateExpectedSql()
    {
        var chatId = Fixture.Create<long>();
        var queryFactory = ServiceProvider.GetRequiredService<IQueryFactory>();
        var chatIdPath = queryFactory.RawDataStructure.Chat.Id;
        var condition = RawData.Lte(chatIdPath, chatId);
        var expectedSql = $"select * from Chats where json_extract(RawData, '{chatIdPath}') <= {chatId}";

        var sql = queryFactory.CreateFindByConditionQuery("Chats", condition);

        sql.Should().Be(expectedSql);
    }

    [Fact]
    public async Task CreateFindByConditionQuery_RawDataContains_CreateExpectedSql()
    {
        var chatNamePart = Fixture.Create<string>();
        var queryFactory = ServiceProvider.GetRequiredService<IQueryFactory>();
        var chatNamePath = queryFactory.RawDataStructure.Chat.Name;
        var condition = RawData.Contains(chatNamePath, chatNamePart);
        var expectedSql = $"select * from Chats where json_extract(RawData, '{chatNamePath}') like '%{chatNamePart}%'";

        var sql = queryFactory.CreateFindByConditionQuery("Chats", condition);

        sql.Should().Be(expectedSql);
    }

    [Fact]
    public async Task CreateFindByConditionQuery_RawDataDoesNotContain_CreateExpectedSql()
    {
        var chatNamePart = Fixture.Create<string>();
        var queryFactory = ServiceProvider.GetRequiredService<IQueryFactory>();
        var chatNamePath = queryFactory.RawDataStructure.Chat.Name;
        var condition = RawData.DoesNotContain(chatNamePath, chatNamePart);
        var expectedSql = $"select * from Chats where json_extract(RawData, '{chatNamePath}') not like '%{chatNamePart}%'";

        var sql = queryFactory.CreateFindByConditionQuery("Chats", condition);

        sql.Should().Be(expectedSql);
    }

    [Fact]
    public async Task CreateFindByConditionQuery_RawDataStartsWith_CreateExpectedSql()
    {
        var chatNamePart = Fixture.Create<string>();
        var queryFactory = ServiceProvider.GetRequiredService<IQueryFactory>();
        var chatNamePath = queryFactory.RawDataStructure.Chat.Name;
        var condition = RawData.StartsWith(chatNamePath, chatNamePart);
        var expectedSql = $"select * from Chats where json_extract(RawData, '{chatNamePath}') like '{chatNamePart}%'";

        var sql = queryFactory.CreateFindByConditionQuery("Chats", condition);

        sql.Should().Be(expectedSql);
    }

    [Fact]
    public async Task CreateFindByConditionQuery_RawDataDoesNotStartWith_CreateExpectedSql()
    {
        var chatNamePart = Fixture.Create<string>();
        var queryFactory = ServiceProvider.GetRequiredService<IQueryFactory>();
        var chatNamePath = queryFactory.RawDataStructure.Chat.Name;
        var condition = RawData.DoesNotStartWith(chatNamePath, chatNamePart);
        var expectedSql = $"select * from Chats where json_extract(RawData, '{chatNamePath}') not like '{chatNamePart}%'";

        var sql = queryFactory.CreateFindByConditionQuery("Chats", condition);

        sql.Should().Be(expectedSql);
    }

    [Fact]
    public async Task CreateFindByConditionQuery_RawDataEndsWith_CreateExpectedSql()
    {
        var chatNamePart = Fixture.Create<string>();
        var queryFactory = ServiceProvider.GetRequiredService<IQueryFactory>();
        var chatNamePath = queryFactory.RawDataStructure.Chat.Name;
        var condition = RawData.EndsWith(chatNamePath, chatNamePart);
        var expectedSql = $"select * from Chats where json_extract(RawData, '{chatNamePath}') like '%{chatNamePart}'";

        var sql = queryFactory.CreateFindByConditionQuery("Chats", condition);

        sql.Should().Be(expectedSql);
    }

    [Fact]
    public async Task CreateFindByConditionQuery_RawDataDoesNotEndWith_CreateExpectedSql()
    {
        var chatNamePart = Fixture.Create<string>();
        var queryFactory = ServiceProvider.GetRequiredService<IQueryFactory>();
        var chatNamePath = queryFactory.RawDataStructure.Chat.Name;
        var condition = RawData.DoesNotEndWith(chatNamePath, chatNamePart);
        var expectedSql = $"select * from Chats where json_extract(RawData, '{chatNamePath}') not like '%{chatNamePart}'";

        var sql = queryFactory.CreateFindByConditionQuery("Chats", condition);

        sql.Should().Be(expectedSql);
    }

    [Fact]
    public async Task CreateFindByConditionQuery_TwoConditions_CreateExpectedSql()
    {
        var chatId = Fixture.Create<long>();
        var chatName = Fixture.Create<string>();
        var queryFactory = ServiceProvider.GetRequiredService<IQueryFactory>();
        var chatIdPath = queryFactory.RawDataStructure.Chat.Id;
        var chatNamePath = queryFactory.RawDataStructure.Chat.Name;
        var condition = RawData.Eq(chatIdPath, chatId).And(RawData.Ne(chatNamePath, chatName));
        var expectedSql = $"select * from Chats where json_extract(RawData, '{chatIdPath}') = {chatId} And json_extract(RawData, '{chatNamePath}') != '{chatName}'";

        var sql = queryFactory.CreateFindByConditionQuery("Chats", condition);

        sql.Should().Be(expectedSql);
    }

    [Fact]
    public async Task CreateFindByConditionQuery_OneOfConditionsInBrackets_CreateExpectedSql()
    {
        var chatId = Fixture.Create<long>();
        var containsValue = Fixture.Create<string>();
        var startsWithValue = Fixture.Create<string>();
        var queryFactory = ServiceProvider.GetRequiredService<IQueryFactory>();
        var chatIdPath = queryFactory.RawDataStructure.Chat.Id;
        var chatNamePath = queryFactory.RawDataStructure.Chat.Name;
        var condition = RawData.Gte(chatIdPath, chatId)
            .And(RawData.Contains(chatNamePath, containsValue)
                .Or(RawData.StartsWith(chatNamePath, startsWithValue)));
        var expectedSql = $"select * from Chats where json_extract(RawData, '{chatIdPath}') >= {chatId} And (json_extract(RawData, '{chatNamePath}') like '%{containsValue}%' Or json_extract(RawData, '{chatNamePath}') like '{startsWithValue}%')";

        var sql = queryFactory.CreateFindByConditionQuery("Chats", condition);

        sql.Should().Be(expectedSql);
    }
}