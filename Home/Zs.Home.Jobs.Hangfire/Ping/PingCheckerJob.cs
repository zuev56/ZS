using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Home.Jobs.Hangfire.Extensions;
using Zs.Home.Jobs.Hangfire.Hangfire;
using Zs.Home.Jobs.Hangfire.Notification;
using Zs.Home.WebApi;
using IPStatus = System.Net.NetworkInformation.IPStatus;

namespace Zs.Home.Jobs.Hangfire.Ping;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PingCheckerJob : IJob
{
    private sealed record Target(string Host, short? Port);

    private static readonly Dictionary<Target, IPStatus> _targetToIpStatusMap = new();
    private readonly IPingClient _pingClient;
    private readonly Notifier _notifier;
    private readonly ILogger<PingCheckerJob> _logger;

    public PingCheckerJob(
        IPingClient pingClient,
        Notifier notifier,
        ILogger<PingCheckerJob> logger)
    {
        _pingClient = pingClient;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogJobStart();

        var connectionLossInfo = await DetectConnectionLosesAsync(ct);

        await _notifier.SendNotificationAsync(connectionLossInfo, ct);

        _logger.LogJobFinish(sw.Elapsed);
    }

    private async Task<string> DetectConnectionLosesAsync(CancellationToken ct)
    {
        var pingResponse = await _pingClient.PingAllAsync(ct);

        var message = new StringBuilder();
        foreach (var pingResult in pingResponse.PingResults)
        {
            var target = new Target(pingResult.Host, (short?)pingResult.Port);
            var ipStatus = (IPStatus)pingResult.Status;

            if (_targetToIpStatusMap.TryGetValue(target, out var lastIpStatus))
            {
                if (lastIpStatus == ipStatus)
                    continue;

                _targetToIpStatusMap[target] = ipStatus;

                if (message.Length > 0)
                    message.Append(Environment.NewLine);

                message.Append($"Connection to '{pingResult.Description}' {(ipStatus == IPStatus.Success ? "restored" : "lost")}.");
            }
            else
                _targetToIpStatusMap.Add(target, ipStatus);
        }

        return message.ToString();
    }
}
