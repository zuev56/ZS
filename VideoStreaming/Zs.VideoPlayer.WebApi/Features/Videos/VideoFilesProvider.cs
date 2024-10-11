using System.IO;
using System.Threading.Tasks;

namespace Zs.VideoPlayer.WebApi.Features.Videos;

public sealed class VideoFilesProvider
{
    private readonly string _workingDirectory;
    private readonly string _videoFileName;

    public VideoFilesProvider(
        string workingDirectory,
        string videoFileName)
    {
        _workingDirectory = workingDirectory;
        _videoFileName = videoFileName;
    }

    public async Task<byte[]?> GetM3u8Async()
    {
        var m3u8FilePath = Path.Combine(_workingDirectory, $"{_videoFileName}.m3u8");

        var m3u8FileBytes = File.Exists(m3u8FilePath)
            ? await File.ReadAllBytesAsync(m3u8FilePath)
            : null;

        return m3u8FileBytes;
    }

    public async Task<byte[]?> GetTsAsync(string tsFileName)
    {
        var tsFilePath = Path.Combine(_workingDirectory, tsFileName);

        var tsFileBytes = File.Exists(tsFilePath)
            ? await File.ReadAllBytesAsync(tsFilePath)
            : null;

        return tsFileBytes;
    }
}
