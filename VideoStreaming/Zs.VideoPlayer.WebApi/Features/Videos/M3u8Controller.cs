using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Zs.VideoPlayer.WebApi.Features.Videos;

[ApiController]
[Route("video/[controller]")]
[Route("[controller]")]
public class M3u8Controller : ControllerBase
{
    private readonly VideoFilesProvider _videoFilesProvider;

    public M3u8Controller(VideoFilesProvider videoFilesProvider)
    {
        _videoFilesProvider = videoFilesProvider;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        Response.Headers.Append("Access-Control-Allow-Origin", "*");
        var m3u8Bytes = await _videoFilesProvider.GetM3u8Async();

        return m3u8Bytes != null
            ? File(m3u8Bytes, "application/octet-stream", enableRangeProcessing: true)
            : NotFound();
    }
}
