using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using File = TagLib.File;

namespace Zs.Home.AIAgent.Worker.Plugins.Music;

/// <summary>
/// Подключаемый модуль для управления музыкой.
/// </summary>
public sealed class MusicPlugin
{
    private readonly Repository _repository;
    private readonly MusicPluginSettings _settings;
    private readonly ILogger<MusicPlugin> _logger;

    public MusicPlugin(Repository repository, IOptions<MusicPluginSettings> settings, ILogger<MusicPlugin> logger)
    {
        _repository = repository;
        _logger = logger;
        _settings = settings.Value;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        var musicFiles = _settings.Paths
            .SelectMany(path => _settings.Extensions.SelectMany(ext =>
                Directory.GetFiles(path, $"*{ext}", SearchOption.AllDirectories)))
            .ToList();

        foreach (var musicFile in musicFiles.Chunk(1000))
        {
            var musicTracks = musicFile
                .Select(path => new MusicTrack
                {
                    FilePath = path,
                    SearchText = BuildSearchText(path)
                })
                .ToList();

            await _repository.AddOrUpdateBatchAsync(musicTracks);
        }

        _logger.LogInformation("Music plugin loaded. {MusicFilesCount} tracks found.", musicFiles.Count);
    }

    [KernelFunction("search_song")]
    [Description("Ищет песни по названию или описанию.")]
    [return: Description("Наиболее актуальные совпадения")]
    public async Task<List<string>> SearchSong(
        [Description("Поисковый запрос, описывающий песню")]
        string query,
        [Description("Максимальное количество результатов для возврата")]
        int limit = 3)
    {
        _logger.LogDebug("Использую функцию search_song");
        var songResults = await _repository.SearchAsync(query, limit);

        return songResults.Select(track => Path.GetFileNameWithoutExtension(track.FilePath)).ToList();
    }

    private string BuildSearchText(string path)
    {
        try
        {
            var file = File.Create(path);
            var tag = file.Tag;
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(tag.FirstPerformer)) sb.Append(tag.FirstPerformer.Trim()).Append(' ');
            if (!string.IsNullOrEmpty(tag.FirstAlbumArtist)) sb.Append(tag.FirstAlbumArtist.Trim()).Append(' ');
            if (!string.IsNullOrEmpty(tag.Title)) sb.Append(tag.Title.Trim()).Append(' ');
            if (!string.IsNullOrEmpty(tag.Album)) sb.Append(tag.Album.Trim()).Append(' ');
            if (tag.Year > 0) sb.Append(tag.Year);

            return string.Join(", ", sb);
        }
        catch (Exception e)
        {
            _logger.LogWarning("Search text building failed for {Path}: {Exception}", path, $"{e.GetType()}. {e.Message}");
            return Path.GetFileNameWithoutExtension(path);
        }
    }
}
