using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;

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

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var weatherDashboardTask = _mediator.Send(new WeatherDashboardQuery(), cancellationToken);
        var pingResultTask = _mediator.Send(new PingResultQuery(), cancellationToken);

        await Task.WhenAll(weatherDashboardTask, pingResultTask);

        WeatherDashboard = weatherDashboardTask.Result;
        PingResult = pingResultTask.Result;
    }

    public async Task<PartialViewResult> OnGetWeatherDashboardAsync(CancellationToken cancellationToken)
    {
        var weatherDashboard = await _mediator.Send(new WeatherDashboardQuery(), cancellationToken);

        return new PartialViewResult {
            ViewName = "_WeatherDashboard",
            ViewData = new ViewDataDictionary<WeatherDashboard>(ViewData, weatherDashboard)
        };
    }

    public async Task<IActionResult> OnGetPingResultAsync(CancellationToken cancellationToken)
    {
        var pingResult = await _mediator.Send(new PingResultQuery(), cancellationToken);

        return new PartialViewResult {
            ViewName = "_PingResult",
            ViewData = new ViewDataDictionary<PingResult>(ViewData, pingResult)
        };
    }
}
