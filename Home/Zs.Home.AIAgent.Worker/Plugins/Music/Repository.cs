using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Zs.Home.AIAgent.Worker.Plugins.Music;

public sealed class Repository
{
    private readonly string _connectionString;

    public Repository(IOptions<MusicPluginSettings> settings)
    {
        _connectionString = settings.Value.ConnectionString;
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task AddOrUpdateBatchAsync(List<MusicTrack> tracks)
    {
        var trackList = tracks.ToList();
        if (!trackList.Any()) return;

        const string sql = @"
            insert into music.tracks (file_path, search_text, search_vector, updated_at)
            values (@filepath, @searchtext, to_tsvector('russian', @searchtext), now())
            on conflict (file_path)
            do update set
                search_text = excluded.search_text,
                search_vector = excluded.search_vector,
                updated_at = now()";

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(sql, trackList);
    }

    public async Task<List<MusicTrack>> SearchAsync(string query, int limit = 50)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetAllAsync(limit);

        var safeQuery = SanitizeQuery(query);

        const string sql = @"
            select
                id,
                file_path as filepath,
                search_text as searchtext,
                ts_rank(search_vector, websearch_to_tsquery('russian', @query)) as rank
            from music.tracks
            where search_vector @@ websearch_to_tsquery('russian', @query)
               or search_vector @@ websearch_to_tsquery('english', @query)
            order by rank desc
            limit @limit";

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<MusicTrack>(sql, new { Query = safeQuery, Limit = limit });
        return result.ToList();
    }

    public async Task<List<MusicTrack>> GetAllAsync(int limit = 1000, int offset = 0)
    {
        const string sql = @"
            select
                id,
                file_path as filepath,
                search_text as searchtext
            from music.tracks
            order by id
            limit @limit offset @offset";

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<MusicTrack>(sql, new { Limit = limit, Offset = offset });
        return result.ToList();
    }

    /// <summary>
    /// Очистка запроса от символов, ломающих tsquery
    /// </summary>
    private static string SanitizeQuery(string query) => string.IsNullOrWhiteSpace(query)
        ? string.Empty
        : Regex.Replace(query, @"[^\w\s\-]", " ");
}
