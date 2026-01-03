using Microsoft.AspNetCore.Mvc;
using Zs.Common.Extensions;

namespace Zs.Home.WebApi.Features;

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
