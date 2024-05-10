using System.Collections.Generic;
using System.Data.Common;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Zs.Common.Extensions;

public static class DbDataReaderExtensions
{
    private static readonly JsonSerializerOptions PrettyJsonSerializerOptions = new ()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static async Task<string> ReadToJsonAsync(this DbDataReader reader, CancellationToken cancellationToken)
    {
        var rows = new List<Dictionary<string, string?>>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var isDbNull = await reader.IsDBNullAsync(0, cancellationToken).ConfigureAwait(false);
            if (isDbNull)
                break;

            var row = new Dictionary<string, string?>();
            var columnSchema = await reader.GetColumnSchemaAsync(cancellationToken).ConfigureAwait(false);

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var columnName = columnSchema[i].ColumnName;
                row.Add(columnName, reader[i].ToString());
            }

            rows.Add(row);
        }

        return JsonSerializer.Serialize(rows, PrettyJsonSerializerOptions);
    }
}