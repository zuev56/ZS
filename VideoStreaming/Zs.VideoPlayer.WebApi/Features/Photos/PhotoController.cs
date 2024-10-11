// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
//
// namespace Zs.VideoPlayer.WebApi.Features.Photos;
//
// [ApiController]
// [Route("[controller]")]
// public class PhotoController : ControllerBase
// {
//     private readonly RtspImageService _rtspImageService;
//
//     public PhotoController(RtspImageService rtspImageService)
//     {
//         _rtspImageService = rtspImageService;
//     }
//
//     [HttpGet]
//     public async Task<IActionResult> Get()
//     {
//         var imageData = await _rtspImageService.CreateImageAsync();
//         return File(imageData, "image/png");
//     }
// }
