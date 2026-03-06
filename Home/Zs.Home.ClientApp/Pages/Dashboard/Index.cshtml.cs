using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Zs.Home.ClientApp.Pages.Dashboard.Ping;
using Zs.Home.ClientApp.Pages.Dashboard.Vk;
using Zs.Home.ClientApp.Pages.Dashboard.Weather;

namespace Zs.Home.ClientApp.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(ILogger<IndexModel> logger, IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<PartialViewResult> OnGetWeatherDashboardAsync(CancellationToken cancellationToken)
    {
        var weatherDashboard = await _mediator.Send(new WeatherDashboardQuery(), cancellationToken);

        return new PartialViewResult {
            ViewName = "Weather/_WeatherDashboard",
            ViewData = new ViewDataDictionary<WeatherDashboard>(ViewData, weatherDashboard)
        };
    }

    public async Task<IActionResult> OnGetPingResultAsync(CancellationToken cancellationToken)
    {
        var pingResult = await _mediator.Send(new PingResultQuery(), cancellationToken);

        return new PartialViewResult {
            ViewName = "Ping/_PingResult",
            ViewData = new ViewDataDictionary<PingResult>(ViewData, pingResult)
        };
    }

    public async Task<PartialViewResult> OnGetVkActivityAsync(CancellationToken cancellationToken)
    {
        var vkActivity = await _mediator.Send(new VkActivityQuery(), cancellationToken);

        return new PartialViewResult {
            ViewName = "Vk/_VkActivity",
            ViewData = new ViewDataDictionary<Vk.VkActivity>(ViewData, vkActivity)
        };
    }
}
