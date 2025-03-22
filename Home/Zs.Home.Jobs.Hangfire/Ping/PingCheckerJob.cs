using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zs.Home.Application.Features.Ping;
using Zs.Home.Jobs.Hangfire.Extensions;
using Zs.Home.Jobs.Hangfire.Notification;

namespace Zs.Home.Jobs.Hangfire.Ping;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PingCheckerJob
{
    private static readonly Dictionary<Target, IPStatus> _targetToIpStatusMap = new();
    private readonly IPingChecker _pingChecker;
    private readonly PingCheckerSettings _settings;
    private readonly Notifier _notifier;
    private readonly ILogger<PingCheckerJob> _logger;


    public PingCheckerJob(
        IPingChecker userWatcher,
        IOptions<PingCheckerSettings> settings,
        Notifier notifier,
        ILogger<PingCheckerJob> logger)
    {
        _pingChecker = userWatcher;
        _notifier = notifier;
        _settings = settings.Value;
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
        if (_settings.Targets.Length == 0)
            return string.Empty;

        var message = new StringBuilder();
        foreach (var target in _settings.Targets)
        {
            var ipStatus = await _pingChecker.PingAsync(target, _settings.Attempts, _settings.Timeout);

            if (_targetToIpStatusMap.TryGetValue(target, out var lastIpStatus))
            {
                if (lastIpStatus == ipStatus)
                    continue;

                _targetToIpStatusMap[target] = ipStatus;

                if (message.Length > 0)
                    message.Append(Environment.NewLine);

                message.Append($"Connection to '{target.Description}' {(ipStatus == IPStatus.Success ? "restored" : "lost")}.");
            }
            else
                _targetToIpStatusMap.Add(target, ipStatus);
        }

        return message.ToString();
    }
}
