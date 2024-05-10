﻿using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zs.VkActivity.Common.Abstractions;
using Zs.VkActivity.Data.Abstractions;

namespace Zs.VkActivity.Worker.IntegrationTests;

[ExcludeFromCodeCoverage]
public sealed class UserManagerTests : TestBase
{
    [Theory]
    [InlineData("zuev56")]
    [InlineData("8790237")]
    public async Task AddUsersAsync_Should_AddOneUser(string userId)
    {
        var userManager = ServiceProvider.GetRequiredService<IUserManager>();

        var addUserResult = await userManager.AddUsersAsync(userId);

        addUserResult.Successful.Should().BeTrue();
        addUserResult.Value.Should().HaveCount(1);
        addUserResult.Value[0].Id.Should().Be(8790237);

        await Task.Delay(1000);
    }

    [Theory]
    [InlineData("zuev56", "1", "15223437")]
    [InlineData("8790237", "durov", "kozlov56")]
    public async Task AddUsersAsync_Should_AddUserArray(params string[] userIds)
    {
        var userManager = ServiceProvider.GetRequiredService<IUserManager>();

        var addUserResult = await userManager.AddUsersAsync(userIds);

        addUserResult.Successful.Should().BeTrue();
        addUserResult.Value.Should().HaveCount(userIds.Length);

        await Task.Delay(1000);
    }

    [Theory(Skip = "BaseRepository.Update is not implemented")]
    [InlineData(8790237)]
    public async Task UpdateUsersAsync_Should_SuccessfullyUpdate(int userId)
    {
        var userManager = ServiceProvider.GetRequiredService<IUserManager>();
        await userManager.AddUsersAsync(userId.ToString());
        var usersRepository = ServiceProvider.GetRequiredService<IUsersRepository>();
        var dbUser = await usersRepository.FindByIdAsync(userId);
        const string outdatedName = "OutdatedFirstName";
        dbUser!.FirstName = outdatedName;
        var isUpdated = await usersRepository.UpdateAsync(dbUser, u => u.Id);
        isUpdated.Should().BeTrue();

        var updateUserResult = await userManager.UpdateUsersAsync(userId);

        updateUserResult.Successful.Should().BeTrue();
        var dbUserAfterUpdate = await usersRepository.FindByIdAsync(userId);
        dbUserAfterUpdate!.FirstName.Should().NotBe(outdatedName);

        await Task.Delay(1000);
    }
}