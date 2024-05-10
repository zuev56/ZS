using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Zs.VideoPlayer.ClientApp.Pages;

public class IndexModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IConfiguration configuration, ILogger<IndexModel> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void OnGet()
    {
    }

    public async Task OnGetResetCommandAsync()
    {
        await Task.WhenAll(new[]
        {
            SetIpCameraDate(),
            ResetRtspToHlsConverter()
        });
    }

    private async Task SetIpCameraDate()
    {
        var cameraIp = _configuration["CameraIp"];
        var time = $"{DateTime.Now:yyyy-MM-dd-HH:mm:ss}";
        var url = $"http://{cameraIp}/cgi-bin/jvsweb.cgi?cmd=webipcinfo settime {time}";
        using var client = new HttpClient();
        await client.GetAsync(url);    
    }


    private static async Task ResetRtspToHlsConverter()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"systemctl restart RtspToHlsConverter\"",
                RedirectStandardOutput = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();
    }
}
