using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zs.Bot.Data.Repositories;
using Zs.Common.Abstractions;

namespace Zs.Bot.Data.SQLite.IntegrationTests;

public sealed class DbClientTests : TestBase
{
    [Fact]
    public async Task GetQueryResultAsync_ReturnsJsonArrayWithSingleIntegerProperty()
    {
        await FillDatabaseWithTestDataSetAsync();
        var dbClient = ServiceProvider.GetRequiredService<IDbClient>();
        var columnName = "count";
        var sql = $"select count(*) as {columnName} from Chats";

        var result = await dbClient.GetQueryResultAsync(sql);

        result.Should().NotBeNullOrWhiteSpace();
        var jsonValue = GetJsonValue(result, columnName);
        var parseAction = () => int.Parse(jsonValue!);
        parseAction.Should().NotThrow();
    }

    private static string? GetJsonValue(string? json, string propertyName)
    {
        if (json is null)
            return null;

        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
        var jsonValue = jsonElement.EnumerateArray().Single().GetProperty(propertyName).ToString();
        return jsonValue;
    }

    [Fact]
    public async Task GetQueryResultAsync_ReturnsJsonArrayWithSingleStringProperty()
    {
        await FillDatabaseWithTestDataSetAsync();
        var dbClient = ServiceProvider.GetRequiredService<IDbClient>();
        var usersRepository = ServiceProvider.GetRequiredService<IUsersRepository>();
        var user = await usersRepository.FindByRawIdAsync(1);
        var expectedUserName = user!.UserName;
        var columnName = "UserName";
        var sql = $"select {columnName} from Users where Id = {user.Id}";

        var result = await dbClient.GetQueryResultAsync(sql);

        result.Should().NotBeNullOrWhiteSpace();
        var jsonValue = GetJsonValue(result, columnName);
        jsonValue.Should().Be(expectedUserName);
    }

    [Fact]
    public async Task GetQueryResultAsync_ReturnsJsonArrayWithComplexItems()
    {
        await FillDatabaseWithTestDataSetAsync();
        var dbClient = ServiceProvider.GetRequiredService<IDbClient>();
        var sql = "select Id, UserName, createdAt from Users";

        var result = await dbClient.GetQueryResultAsync(sql);

        result.Should().NotBeNullOrWhiteSpace();
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(result);
        var items = jsonElement.EnumerateArray().Select(e => e.EnumerateObject().ToArray())
            .Select(item => new
            {
                Id = int.Parse(item[0].Value.ToString()),
                Name = item[1].Value.ToString(),
                CreatedAt = item[2].Value.ToString()
            });
        items.Should().NotBeEmpty();
    }
}