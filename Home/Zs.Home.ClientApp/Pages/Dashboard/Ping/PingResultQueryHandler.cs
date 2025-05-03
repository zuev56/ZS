using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zs.Home.WebApi;
using IPStatus = System.Net.NetworkInformation.IPStatus;

namespace Zs.Home.ClientApp.Pages.Dashboard.Ping;

public sealed class PingResultQueryHandler : IRequestHandler<PingResultQuery, PingResult>
{
    private readonly IPingClient _pingClient;

    public PingResultQueryHandler(IPingClient pingClient)
    {
        _pingClient = pingClient;
    }

    public async Task<PingResult> Handle(PingResultQuery request, CancellationToken cancellationToken)
    {
        var pingResponse = await _pingClient.PingAllAsync(cancellationToken);
        var targets = pingResponse.PingResults
            .Select(r =>
            {
                var targetName = r.Description ?? r.Host + (r.Port.HasValue ? $":{r.Port}" : string.Empty);
                return new Target(targetName, (IPStatus)r.Status == IPStatus.Success);
            })
            .ToList();

        return new PingResult { Targets = targets };
    }
}
