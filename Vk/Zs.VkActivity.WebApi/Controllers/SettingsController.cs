using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Zs.Common.Extensions;

namespace Zs.VkActivity.WebApi.Controllers;

[Route("api/[controller]")]
public sealed class SettingsController : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetAllSettings([FromServices] IConfiguration configuration)
    {
        var settingsResult = Ok(configuration.ToFormattedString());

        return Task.FromResult<IActionResult>(settingsResult);
    }
}
