using System.Text.Json;
using System.Threading.Tasks;
using Npgsql;
using Zs.Common.Models;

namespace Zs.Common.Data.Postgres.Services;

public static class DbInfoService
{
    public static async Task<DbTableInfo[]> GetInfoAsync(string connectionString, string schemaName)
    {
        var tablesInfoQuery = $@"
                 select (with userQuery as (
                    SELECT
                        t.table_name as table,
                        (xpath('/row/cnt/text()', xml_count))[1]::text::int AS rows,
                        ROUND(pg_total_relation_size('""' || t.table_schema || '"".""' || t.table_name || '""') / (1024.0 * 1024.0), 1)::text || ' MB' AS size
                    FROM information_schema.tables t
                    LEFT JOIN LATERAL (
                        SELECT table_schema, table_name, query_to_xml(format('SELECT count(*) as cnt FROM %I.%I', table_schema, table_name), false, true, '') AS xml_count
                        FROM information_schema.tables
                        WHERE table_schema = t.table_schema AND table_name = t.table_name
                    ) AS sub ON true
                    WHERE t.table_schema = '{schemaName}' AND t.table_type = 'BASE TABLE'
                ) select json_agg(q) from userQuery q)";

        await using var dbConnection = new NpgsqlConnection(connectionString);
        await dbConnection.OpenAsync();
        await using var command = new NpgsqlCommand(tablesInfoQuery, dbConnection);

        var result = await command.ExecuteScalarAsync();
        if (result is string value)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<DbTableInfo[]>(value, options)!;
        }

        return [];
    }
}
