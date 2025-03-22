using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using Zs.Home.Application.Features.Ping;

namespace Zs.Home.ClientApp.Pages.Dashboard.Ping;

public sealed class PingResultQueryHandler : IRequestHandler<PingResultQuery, PingResult>
{
    private readonly IPingChecker _pingChecker;
    private readonly PingCheckerSettings _settings;

    public PingResultQueryHandler(IPingChecker pingChecker, IOptions<PingCheckerSettings> settings)
    {
        _pingChecker = pingChecker;
        _settings = settings.Value;
    }

    public async Task<PingResult> Handle(PingResultQuery request, CancellationToken cancellationToken)
    {
        var targetStatuses = await PingTargetsAsync();

        return new PingResult { Targets = targetStatuses };
    }

    private async Task<IReadOnlyList<Target>> PingTargetsAsync()
    {
        if (_settings.Targets.Length == 0)
            return [];

        var targetClientModels = new ConcurrentBag<Target>();
        await Parallel.ForEachAsync(_settings.Targets, async (target, _) =>
        {
            var hostStatus = await _pingChecker.PingAsync(target, _settings.Attempts, _settings.Timeout);
            var targetName = target.Description ?? target.Socket;

            targetClientModels.Add(new Target(targetName, hostStatus == IPStatus.Success));
        });

        return targetClientModels
            .OrderBy(t => t.Name)
            .ToList();
    }
}
