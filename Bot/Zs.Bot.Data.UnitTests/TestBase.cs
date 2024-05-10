using System;
using System.Threading;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Zs.Bot.Data.Models;

namespace Zs.Bot.Data.UnitTests;

public abstract class TestBase
{
    protected readonly IFixture Fixture;

    protected TestBase()
    {
        Fixture = new Fixture();
        Fixture.Customize(new AutoNSubstituteCustomization());
    }

    protected IDbContextFactory<TestBotContext> CreateBotContextFactory()
    {
        var dbName = $"InMemoryDB_{Guid.NewGuid()}";
        var dbContextFactory = Substitute.For<IDbContextFactory<TestBotContext>>();
        dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(x => CreateDbContext(dbName));

        Fixture.Inject(dbContextFactory);

        return dbContextFactory;
    }

    private static TestBotContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<TestBotContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new TestBotContext(options);
    }
}

public sealed class TestBotContext : BotContext<TestBotContext>
{
    public DbSet<TestDbEntity> TestEntities { get; set; } = null!;
    public TestBotContext(DbContextOptions<TestBotContext> options)
        : base(options)
    {
    }
}

public sealed class TestDbEntity : DbEntity {}