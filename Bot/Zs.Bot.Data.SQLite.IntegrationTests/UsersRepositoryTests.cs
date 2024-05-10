using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zs.Bot.Data.Repositories;

namespace Zs.Bot.Data.SQLite.IntegrationTests;

public sealed class UsersRepositoryTests : TestBase
{
    [Fact]
    public async Task FindByRawIdAsync_UserWithRawIdExists_ReturnsUser()
    {
        await FillDatabaseWithTestDataSetAsync();
        var usersRepository = ServiceProvider.GetRequiredService<IUsersRepository>();
        var existingRawUserId = 1;

        var user = await usersRepository.FindByRawIdAsync(existingRawUserId);

        user.Should().NotBeNull();
    }

    [Fact]
    public async Task FindByRawIdAsync_UserWithRawIdDoesNotExist_ReturnsNull()
    {
        var usersRepository = ServiceProvider.GetRequiredService<IUsersRepository>();
        var nonExistentRawUserId = 1;

        var user = await usersRepository.FindByRawIdAsync(nonExistentRawUserId);

        user.Should().BeNull();
    }

    [Fact]
    public async Task FindByRolesAsync_UsersWithRoleExist_ReturnsUsers()
    {
        await FillDatabaseWithTestDataSetAsync(1000);
        var usersRepository = ServiceProvider.GetRequiredService<IUsersRepository>();
        var existingUsers = await usersRepository.FindAllAsync();
        var existingRoles = existingUsers.Take(10).Select(u => u.Role).ToArray();

        var user = await usersRepository.FindByRolesAsync(existingRoles);

        user.Should().NotBeNullOrEmpty()
            .And.OnlyContain(u => existingRoles.Contains(u.Role));
    }
}