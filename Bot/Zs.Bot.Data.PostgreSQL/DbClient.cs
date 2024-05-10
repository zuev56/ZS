using Microsoft.Extensions.Logging;
using Npgsql;
using Zs.Common.Abstractions;

namespace Zs.Bot.Data.PostgreSQL;

public sealed class DbClient : DbClientBase<NpgsqlConnection, NpgsqlCommand>
{
    public DbClient(string connectionString, ILogger<DbClient>? logger = null)
        : base(connectionString, logger)
    {
    }
}