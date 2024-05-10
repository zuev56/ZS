using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Zs.Bot.Data.Repositories;
using Zs.Bot.Data.Queries;

namespace Zs.Bot.Data.UnitTests.Repositories;

public sealed class CommonRepositoryTests : TestBase
{
    private sealed class TestRepository<TContext> : CommonRepository<TContext, TestDbEntity> where TContext : DbContext
    {
        public TestRepository(IDbContextFactory<TContext> contextFactory, IQueryFactory queryFactory)
            : base(contextFactory, queryFactory, new NullLogger<TestRepository<TContext>>())
        {
        }
    }

    [Fact]
    public async Task ExistsAsync_ItemExists_ReturnsTrue()
    {
        var contextFactory = CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var testItems = await FillDatabaseWithTestItems(contextFactory, entityCount: 100);
        var existingId = testItems.First().Id;

        var exists = await commonRepository.ExistsAsync(existingId);

        exists.Should().BeTrue();
    }

    private async Task<IReadOnlyList<TestDbEntity>> FillDatabaseWithTestItems(IDbContextFactory<TestBotContext> contextFactory, int entityCount)
    {
        var testItems = Fixture.CreateMany<TestDbEntity>(entityCount).ToArray();

        await using var context = await contextFactory.CreateDbContextAsync();
        context.Set<TestDbEntity>().AddRange(testItems);
        await context.SaveChangesAsync();

        return testItems;
    }

    [Fact]
    public async Task ExistsAsync_ItemDoesNotExist_ReturnsFalse()
    {
        CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var nonExistentId = 123;

        var exists = await commonRepository.ExistsAsync(nonExistentId);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ReturnsCorrectCountWithPredicate()
    {
        var contextFactory = CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var testItems = await FillDatabaseWithTestItems(contextFactory, entityCount: 100);
        Expression<Func<TestDbEntity, bool>> predicate = i => i.Id % 2 == 0;
        var compiledPredicate = predicate.Compile();
        var specificIdCount = testItems.Count(compiledPredicate);

        var resultCount = await commonRepository.CountAsync(predicate);

        resultCount.Should().Be(specificIdCount);
    }

    [Fact]
    public async Task CountAsync_WithoutPredicate_ReturnsCorrectCount()
    {
        var contextFactory = CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var testItems = await FillDatabaseWithTestItems(contextFactory, entityCount: 100);
        var realCount = testItems.Count;

        var resultCount = await commonRepository.CountAsync();

        resultCount.Should().Be(realCount);
    }

    [Fact]
    public async Task FindAsync_ItemWithTheIdExists_ReturnsTheItem()
    {
        var contextFactory = CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var testItems = await FillDatabaseWithTestItems(contextFactory, entityCount: 100);
        var firstItem = testItems.First();
        var existingId = firstItem.Id;

        var resultItem = await commonRepository.FindAsync(existingId);

        resultItem.Should().BeEquivalentTo(firstItem);
    }

    [Fact]
    public async Task FindAsync_ItemWithTheIdDoesNotExist_ReturnsNull()
    {
        var contextFactory = CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var testItems = await FillDatabaseWithTestItems(contextFactory, entityCount: 100);
        var nonexistentId = testItems.MaxBy(i => i.Id)!.Id + 1;

        var resultItem = await commonRepository.FindAsync(nonexistentId);

        resultItem.Should().BeNull();
    }

    [Fact]
    public async Task FindAsync_ItemByPredicateExists_ReturnsTheItem()
    {
        var contextFactory = CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var testItems = await FillDatabaseWithTestItems(contextFactory, entityCount: 100);
        Expression<Func<TestDbEntity, bool>> predicate = i => i.Id % 2 == 0;
        var compiledPredicate = predicate.Compile();
        var item = testItems.First(compiledPredicate);

        var resultItem = await commonRepository.FindAsync(predicate);

        resultItem.Should().BeEquivalentTo(item);
    }

    [Fact]
    public async Task FindAsync_ItemByPredicateDoesNotExist_ReturnsNull()
    {
        var contextFactory = CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var testItems = await FillDatabaseWithTestItems(contextFactory, entityCount: 100);
        var nonexistentId = testItems.MaxBy(i => i.Id)!.Id + 1;
        Expression<Func<TestDbEntity, bool>> predicate = i => i.Id == nonexistentId;

        var resultItem = await commonRepository.FindAsync(predicate);

        resultItem.Should().BeNull();
    }

    [Fact]
    public async Task FindAllAsync_NoPredicate_ReturnsAllItems()
    {
        var contextFactory = CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var testItems = await FillDatabaseWithTestItems(contextFactory, entityCount: 100);

        var resultItem = await commonRepository.FindAllAsync();

        resultItem.Should().BeEquivalentTo(testItems);
    }

    [Fact]
    public async Task FindAllAsync_ItemsByPredicateExist_ReturnsAllItemsByPredicate()
    {
        var contextFactory = CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var testItems = await FillDatabaseWithTestItems(contextFactory, entityCount: 100);
        Expression<Func<TestDbEntity, bool>> predicate = i => i.Id % 2 == 0;
        var compiledPredicate = predicate.Compile();
        var items = testItems.Where(compiledPredicate);

        var resultItems = await commonRepository.FindAllAsync(predicate);

        resultItems.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task FindAllAsync_ItemsByPredicateDoNotExist_ReturnsEmptyList()
    {
        CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        Expression<Func<TestDbEntity, bool>> predicate = i => i.Id > 0;

        var resultItems = await commonRepository.FindAllAsync(predicate);

        resultItems.Should().BeEmpty();
    }

    [Fact]
    public async Task FindAllAsync_ItemsWithIdsExist_ReturnsItems()
    {
        var contextFactory = CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var testItems = await FillDatabaseWithTestItems(contextFactory, entityCount: 100);
        var existingIds = testItems.Select(i => i.Id).Take(50).ToArray();

        var resultItems = await commonRepository.FindAllAsync(existingIds);

        resultItems.Should().HaveSameCount(existingIds);
        resultItems.Should().OnlyContain(i => existingIds.Contains(i.Id));
    }

    [Fact]
    public async Task FindAllAsync_ItemsWithIdsDoNotExist_ReturnsEmptyList()
    {
        CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var nonExistentIds = new long[] { 123, 456, 789 };

        var resultItems = await commonRepository.FindAllAsync(nonExistentIds);

        resultItems.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_NewItem_SuccessfullySaves()
    {
        CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var newItem = Fixture.Create<TestDbEntity>();

        var saved = await commonRepository.AddAsync(newItem);

        saved.Should().Be(1);
        var itemFromDb = await commonRepository.FindAsync(newItem.Id);
        itemFromDb.Should().NotBeNull().And.BeEquivalentTo(newItem);
    }

    [Fact]
    public async Task AddAsync_ExistingItem_ThrowsArgumentException()
    {
        var contextFactory = CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var testItems = await FillDatabaseWithTestItems(contextFactory, entityCount: 100);
        var itemToUpdate = testItems.First();

        var action = async () => await commonRepository.AddAsync(itemToUpdate);

        await action.Should().ThrowExactlyAsync<ArgumentException>();
    }

    [Fact]
    public async Task AddAsync_Null_ThrowArgumentNullException()
    {
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();

        var action = () => commonRepository.AddAsync(null!);

        await action.Should().ThrowExactlyAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AddRangeAsync_NewItems_SuccessfullySaves()
    {
        CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var itemsCount = 100;
        var newItems = Fixture.CreateMany<TestDbEntity>(itemsCount).ToArray();

        var saved = await commonRepository.AddRangeAsync(newItems);

        saved.Should().Be(itemsCount);
        var itemsFromDb = await commonRepository.FindAllAsync();
        itemsFromDb.Should().BeEquivalentTo(newItems);
    }

    [Fact]
    public async Task AddRangeAsync_ExistingItems_ThrowsArgumentException()
    {
        var contextFactory = CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var testItems = await FillDatabaseWithTestItems(contextFactory, entityCount: 100);
        var itemsCount = 30;
        var itemsToUpdate = testItems.Take(itemsCount).ToList();

        var action = async () => await commonRepository.AddRangeAsync(itemsToUpdate);

        await action.Should().ThrowExactlyAsync<ArgumentException>();
    }

    [Fact]
    public async Task AddRangeAsync_Null_ThrowArgumentNullException()
    {
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();

        var action = () => commonRepository.AddRangeAsync(null!);

        await action.Should().ThrowExactlyAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DeleteAsync_Null_ThrowArgumentNullException()
    {
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();

        var action = () => commonRepository.DeleteAsync(null!);

        await action.Should().ThrowExactlyAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DeleteAsync_ExistingItem_Returns1()
    {
        var contextFactory = CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var testItems = await FillDatabaseWithTestItems(contextFactory, entityCount: 100);
        var itemToDelete = testItems.First();

        var deleted = await commonRepository.DeleteAsync(itemToDelete);

        deleted.Should().Be(1);
        var itemFromDb = await commonRepository.FindAsync(itemToDelete.Id);
        itemFromDb.Should().BeNull();

        var context = await contextFactory.CreateDbContextAsync();
        var resultItemsCount = await context.Set<TestDbEntity>().CountAsync();
        resultItemsCount.Should().Be(testItems.Count - 1);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentItem_ThrowsDbUpdateConcurrencyException()
    {
        CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var itemToDelete = Fixture.Create<TestDbEntity>();

        var action = async () => await commonRepository.DeleteAsync(itemToDelete);

        await action.Should().ThrowExactlyAsync<DbUpdateConcurrencyException>();
    }

    [Fact]
    public async Task DeleteRangeAsync_Null_ThrowArgumentNullException()
    {
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();

        var action = () => commonRepository.DeleteRangeAsync(null!);

        await action.Should().ThrowExactlyAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DeleteRangeAsync_AllItemsExist_ReturnsItemsCount()
    {
        var contextFactory = CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var testItems = await FillDatabaseWithTestItems(contextFactory, entityCount: 100);

        var deleted = await commonRepository.DeleteRangeAsync(testItems);

        deleted.Should().Be(testItems.Count);
        var context = await contextFactory.CreateDbContextAsync();
        var resultItemsCount = await context.Set<TestDbEntity>().CountAsync();
        resultItemsCount.Should().Be(0);
    }

    [Fact]
    public async Task DeleteRangeAsync_NotAllItemsExist_ReturnsLessThenItemsCount()
    {
        var contextFactory = CreateBotContextFactory();
        var commonRepository = Fixture.Create<TestRepository<TestBotContext>>();
        var testItems = await FillDatabaseWithTestItems(contextFactory, entityCount: 100);
        var needToDelete = 30;
        var itemsToDelete = testItems.Take(needToDelete).ToArray();

        var deleted = await commonRepository.DeleteRangeAsync(itemsToDelete);

        deleted.Should().Be(needToDelete);
        var context = await contextFactory.CreateDbContextAsync();
        var resultItemsCount = await context.Set<TestDbEntity>().CountAsync();
        resultItemsCount.Should().Be(testItems.Count - needToDelete);
    }
}