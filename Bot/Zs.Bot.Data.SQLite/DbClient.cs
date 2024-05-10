using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Zs.Common.Abstractions;

namespace Zs.Bot.Data.SQLite;

public sealed class DbClient : DbClientBase<SqliteConnection, SqliteCommand>
{
    public DbClient(string connectionString, ILogger<DbClient>? logger = null)
        : base(connectionString, logger)
    {
    }
}