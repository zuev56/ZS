using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using Zs.Common.Extensions;
using Zs.Home.Application.Features.Ping;

namespace Zs.Home.ClientApp.Pages.Dashboard;

public sealed class PingResultQueryHandler : IRequestHandler<PingResultQuery, PingResult>
{
    private readonly PingCheckerSettings _settings;

    public PingResultQueryHandler(IOptions<PingCheckerSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<PingResult> Handle(PingResultQuery request, CancellationToken cancellationToken)
    {
        var targetStatuses = await GetCurrentStateAsync(5000.Milliseconds());

        return new PingResult { Targets = targetStatuses };
    }

    private async Task<IReadOnlyList<Target>> GetCurrentStateAsync(TimeSpan timeout)
    {
        if (_settings.Targets.Length == 0)
            return [];

        var targetClientModels = new ConcurrentBag<Target>();

        await Parallel.ForEachAsync(_settings.Targets, async (target, cancellationToken) =>
        {
            var hostStatus = await PingAsync(target, timeout)
                .ContinueWith(task =>
                {
                    if (!task.IsFaulted)
                        return task.Result;

                    return task.Exception!.InnerExceptions.SingleOrDefault() is TimeoutException
                        ? IPStatus.TimedOut
                        : throw task.Exception;

                }, TaskContinuationOptions.None);

            var targetName = target.Description ?? target.Host + (target.Port.HasValue ? $":{target.Port}" : string.Empty);

            targetClientModels.Add(new Target(targetName, hostStatus == IPStatus.Success));
        });

        return targetClientModels
            .OrderBy(t => t.Name)
            .ToList();
    }

    private static async Task<IPStatus> PingAsync(Application.Features.Ping.Target target, TimeSpan timeout)
    {
        return target.Port.HasValue
            ? await PingAsync(target.Host, target.Port.Value, timeout)
            : await PingAsync(target.Host, timeout);
    }
    private static async Task<IPStatus> PingAsync(string host, TimeSpan timeout)
    {
        try
        {
            using var ping = new Ping();
            var pingReply = await ping.SendPingAsync(host, timeout).ConfigureAwait(false);

            return pingReply.Status;
        }
        catch
        {
            return IPStatus.Unknown;
        }
    }

    private static async Task<IPStatus> PingAsync(string host, int port, TimeSpan timeout)
    {
        try
        {
            var pingTask = Task.Run(() => {using var client = new TcpClient(host, port); });
            await Task.WhenAny(pingTask, Task.Delay(timeout));

            return pingTask.IsCompletedSuccessfully ? IPStatus.Success : IPStatus.Unknown;
        }
        catch
        {
            return IPStatus.Unknown;
        }
    }
}
