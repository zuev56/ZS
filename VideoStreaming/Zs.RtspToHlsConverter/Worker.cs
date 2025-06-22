using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Zs.RtspToHlsConverter;

public sealed class Worker : BackgroundService
{
    private readonly string _ffmpegCommand;
    private readonly ILogger<Worker> _logger;

    public Worker(IConfiguration configuration, ILogger<Worker> logger)
    {
        var ffmpegPath = configuration["FFmpegPath"];
        var rtspStreamUri = configuration["RtspStreamUri"];
        var videosDirectory = configuration["VideosDirectory"];
        var videoFilesName = configuration["VideoFilesName"];
        var targetSegmentLength = configuration["TargetSegmentLength"];
        var maxItemsInPlaylist = configuration["MaxItemsInPlaylist"];

        _ffmpegCommand = $"{ffmpegPath} -i {rtspStreamUri}"
                + " -fflags flush_packets"                // Write out packets immediately
                + " -max_delay 5"                         // Maximum muxing or demuxing delay in microseconds
                + " -flags -global_header"                // Places global headers in extradata instead of every keyframe
                + $" -hls_time {targetSegmentLength}"     // Set the target segment length in seconds (default = 2)
                + $" -hls_list_size {maxItemsInPlaylist}" // Set the maximum number of playlist entries (default = 5)
                + " -hls_flags delete_segments"           // Segment files removed from the playlist are deleted after a period of time equal to the duration of the segment plus the duration of the playlist
                + " -vcodec copy"                         // Copy the bitstream of the video to the output (gives you the same quality, as nothing will be changed in the video bitstream)
                + $" -y {videosDirectory}/{videoFilesName}.m3u8"
                + " -hide_banner -loglevel error";         // setup logging

        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await RunFfmpegAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(default, exception: ex, null);
            }
        }
    }

    private async Task RunFfmpegAsync(CancellationToken ct)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            await RunFfmpeg("/bin/bash", ct);
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await RunFfmpeg("powershell.exe", ct);
            return;
        }

        throw new NotSupportedException("Not supported OS");
    }

    private async Task RunFfmpeg(string shellPath, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = shellPath,
                Arguments = $"-c \"{_ffmpegCommand}\"",
                RedirectStandardOutput = true
            }
        };
        process.Start();
        await process.WaitForExitAsync(ct);
    }
}
