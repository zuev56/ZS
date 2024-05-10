using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zs.Bot.Data.Repositories;

namespace Zs.Bot.Data.SQLite.IntegrationTests;

public sealed class ChatsRepositoryTests : TestBase
{
    [Fact]
    public async Task FindByRawIdAsync_ChatWithRawIdExists_ReturnsChat()
    {
        await FillDatabaseWithTestDataSetAsync();
        var chatsRepository = ServiceProvider.GetRequiredService<IChatsRepository>();
        var existingRawChatId = 1;

        var chat = await chatsRepository.FindByRawIdAsync(existingRawChatId);

        chat.Should().NotBeNull();
    }

    [Fact]
    public async Task FindByRawIdAsync_ChatWithRawIdDoesNotExist_ReturnsNull()
    {
        var chatsRepository = ServiceProvider.GetRequiredService<IChatsRepository>();
        var nonExistentRawChatId = 1;

        var chat = await chatsRepository.FindByRawIdAsync(nonExistentRawChatId);

        chat.Should().BeNull();
    }
}