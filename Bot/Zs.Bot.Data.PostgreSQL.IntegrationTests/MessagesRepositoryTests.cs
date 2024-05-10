using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zs.Bot.Data.Repositories;
using Zs.Common.Models;

namespace Zs.Bot.Data.PostgreSQL.IntegrationTests;

public sealed class MessagesRepositoryTests : TestBase
{

    [Fact]
    public async Task FindWithTextAsync_ThereIsMessageWithSearchText_ReturnsTheMessage()
    {
        await FillDatabaseWithTestDataSetAsync();
        var messagesRepository = ServiceProvider.GetRequiredService<IMessagesRepository>();

        var allMessages = await messagesRepository.FindAllAsync();
        var message = allMessages.GetRandomValue();
        var rawChatId = message.ChatId;
        var searchTextStartIndex = message.RawData.IndexOf("\"Text\":", StringComparison.InvariantCulture) + 15;
        var searchTextEndIndex = searchTextStartIndex + 10;
        var searchText = message.RawData[searchTextStartIndex..searchTextEndIndex];
        var dateRange = new DateTimeRange(new DateTime(2023, 01, 01), new DateTime(2023, 12, 31));

        var messages = await messagesRepository.FindWithTextAsync(rawChatId, searchText, dateRange);

        messages.Should().NotBeNull();
        messages.Should().Contain(m => m.Id == message.Id);
    }

}