// using System;
// using System.IO;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Logging;
// using Nager.VideoStream;
//
// namespace Zs.VideoPlayer.WebApi.Features.Photos;
//
// public sealed class RtspImageService
// {
//     private readonly VideoStreamClient _videoStreamClient;
//     private readonly string _rtspStreamUri;
//     private readonly string _imagesDirectory;
//     private readonly ILogger<RtspImageService> _logger;
//
//     public RtspImageService(
//         VideoStreamClient videoStreamClient,
//         string rtspStreamUri,
//         string imagesDirectory,
//         ILogger<RtspImageService> logger)
//     {
//         _videoStreamClient = videoStreamClient;
//         _rtspStreamUri = rtspStreamUri;
//         _imagesDirectory = imagesDirectory;
//         _logger = logger;
//     }
//
//     public async Task<byte[]> CreateImageAsync()
//     {
//         var inputSource = new StreamInputSource(_rtspStreamUri);
//         //var inputSource = new WebcamInputSource("Microsoft® LifeCam HD-3000");
//
//         var cts = new CancellationTokenSource();
//         var imagePath = Path.Combine(_imagesDirectory, $"img_{DateTime.Now:yyMMdd_HHmmss}.png");
//         _videoStreamClient.NewImageReceived += NewImageReceived;
//
//         await _videoStreamClient.StartFrameReaderAsync(inputSource, OutputImageFormat.Png, cts.Token);
//
//         return File.ReadAllBytes(imagePath);
//
//         void NewImageReceived(byte[] imageData)
//         {
//             try
//             {
//                 File.WriteAllBytes(imagePath, imageData);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex.StackTrace); // don't recognize extension method for an exception
//             }
//             finally
//             {
//                 _videoStreamClient.NewImageReceived -= NewImageReceived;
//                 cts.Cancel();
//             }
//         }
//     }
// }
