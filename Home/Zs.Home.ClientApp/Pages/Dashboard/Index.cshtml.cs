using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        WeatherDashboard = await _mediator.Send(new WeatherDashboardQuery(), cancellationToken);
    }
}
