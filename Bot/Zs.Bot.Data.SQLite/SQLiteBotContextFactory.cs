using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Zs.Bot.Data.SQLite;

public sealed class SQLiteBotContextFactory : IDbContextFactory<SQLiteBotContext>, IDesignTimeDbContextFactory<SQLiteBotContext>
{
    private readonly DbContextOptions<SQLiteBotContext> _options = null!;

    public SQLiteBotContextFactory()
    {
    }

    public SQLiteBotContextFactory(DbContextOptions<SQLiteBotContext> options)
    {
        _options = options;
    }

    public SQLiteBotContext CreateDbContext() => new(_options);

    // For migrations
    public SQLiteBotContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SQLiteBotContext>();
        optionsBuilder.UseSqlite("Data Source=database.db;");

        return new SQLiteBotContext(optionsBuilder.Options);
    }
}