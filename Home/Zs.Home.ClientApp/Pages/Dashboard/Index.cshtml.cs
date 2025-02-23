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
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public WeatherDashboard WeatherDashboard { get; private set; }
    public PingResult PingResult { get; private set; }
    public Vk.VkActivity VkActivity { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var weatherDashboardTask = _mediator.Send(new WeatherDashboardQuery(), cancellationToken);
        var pingResultTask = _mediator.Send(new PingResultQuery(), cancellationToken);
        var vkActivity = _mediator.Send(new VkActivityQuery(), cancellationToken);

        await Task.WhenAll(weatherDashboardTask, pingResultTask, vkActivity);

        WeatherDashboard = weatherDashboardTask.Result;
        PingResult = pingResultTask.Result;
        VkActivity = vkActivity.Result;
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
