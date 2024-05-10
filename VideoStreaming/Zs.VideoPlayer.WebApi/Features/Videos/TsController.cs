using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Zs.VideoPlayer.WebApi.Features.Videos;

[ApiController]
[Route("video")]
[Route("[controller]")]
public class TsController : ControllerBase
{
    private readonly VideoFilesProvider _videoFilesProvider;

    public TsController(VideoFilesProvider videoFilesProvider)
    {
        _videoFilesProvider = videoFilesProvider;
    }

    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetAsync(string fileName)
    {
        Response.Headers.Add("Access-Control-Allow-Origin", "*");
        var tsBytes = await _videoFilesProvider.GetTsAsync(fileName);

        return tsBytes != null
            ? File(tsBytes, "application/octet-stream", enableRangeProcessing: true)
            : NotFound();
    }
}