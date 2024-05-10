using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;

namespace Zs.Common.Abstractions;

public abstract class DbClientBase<TConnection, TCommand> : IDbClient
    where TConnection : DbConnection, new()
    where TCommand : DbCommand, new()
{
    private readonly string _connectionString;
    private readonly ILogger<DbClientBase<TConnection, TCommand>>? _logger;

    protected DbClientBase(string connectionString, ILogger<DbClientBase<TConnection, TCommand>>? logger = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionString);

        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<string?> GetQueryResultAsync(string sqlQuery, CancellationToken cancellationToken = default)
    {
        return await MakeQueryAndHandleResult(sqlQuery, HandleResult, cancellationToken).ConfigureAwait(false);

        async Task<string?> HandleResult(DbDataReader reader)
            => await reader.ReadToJsonAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<TResult> MakeQueryAndHandleResult<TResult>(
        string sqlQuery, Func<DbDataReader, Task<TResult>> handleResult, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(sqlQuery);

            var sw = Stopwatch.StartNew();
            await using var connection = new TConnection();
            {
                connection.ConnectionString = _connectionString;
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                TResult result;

                await using var command = new TCommand();
                {
                    command.CommandText = sqlQuery;
                    command.Connection = connection;

                    await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                    result = await handleResult.Invoke(reader);
                }

                sw.Stop();
                _logger?.LogDebug("{MethodName} [Elapsed: {Elapsed}].\n\tSQL: {SQL}", nameof(GetQueryResultAsync), sw.Elapsed, sqlQuery);

                await connection.CloseAsync().ConfigureAwait(false);
                return result;
            }
        }
        catch (InvalidCastException ex)
        {
            _logger?.LogError(ex, "Error getting query result");
            throw;
        }
    }
}