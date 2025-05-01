using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Zs.Common.Models;

namespace Zs.Home.WebApi.Features;

[Route("api/[controller]")]
public sealed class HealthCheckController : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetHealthInfo()
    {
        var currentProcess = Process.GetCurrentProcess();
        var healthStatus = HealthStatus.Get(currentProcess, []);

        return Ok(healthStatus);
    }

    [HttpHead]
    public async Task<IActionResult> IsHealthy()
    {
        return Ok();
    }
}
