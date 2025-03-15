using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Zs.Common.Data.Postgres.Services;
using Zs.Common.Models;
using Zs.VkActivity.Common;

namespace Zs.VkActivity.WebApi.Controllers;

[Route("api/[controller]")]
public sealed class HealthCheckController : Controller
{
    // TODO: Try HealthCheckApplicationBuilderExtensions _healthCheckApplicationBuilderExtensions;
    private readonly string _connectionString;

    public HealthCheckController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString(AppSettings.ConnectionStrings.Default)!;
    }

    [HttpGet]
    [HttpHead]
    public async Task<IActionResult> GetHealthInfo()
    {
        if (Request.Method == "HEAD")
            return Ok();

        var currentProcess = Process.GetCurrentProcess();
        var dbTables = await DbInfoService.GetInfoAsync(_connectionString, "vk");
        var healthStatus = HealthStatus.Get(currentProcess, dbTables);

        return Ok(healthStatus);
    }
}
