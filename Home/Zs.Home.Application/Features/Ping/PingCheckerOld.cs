using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Zs.Common.Extensions;
using Zs.Common.Services.Scheduling;

namespace Zs.Home.Application.Features.Ping;

internal sealed class PingCheckerOld : IPingChecker
{
    private const int AttemptsWhenNotReachable = 3;
    private static readonly TimeSpan BaseDelay = 300.Milliseconds();
    private readonly PingCheckerSettings _settings;
    private readonly Dictionary<Target, bool> _targetToReachabilityMap = new();

    // TODO: Почему джоб лежит не в боте, если он нужен только там?
    public ProgramJob<string> Job { get; }

    public PingCheckerOld(IOptions<PingCheckerSettings> options)
    {
        _settings = options.Value;

        Job = new ProgramJob<string>(
            period: 20.Seconds(),
            method: PingTargetsAsync,
            startUtcDate: DateTime.UtcNow + 7.Seconds());
    }

    private async Task<string> PingTargetsAsync()
    {
        var message = new StringBuilder();
        foreach (var target in _settings.Targets)
        {
            var isReachable = await IsReachable(target);

            if (_targetToReachabilityMap.TryGetValue(target, out var lastIsReachable))
            {
                if (lastIsReachable == isReachable)
                    continue;

                _targetToReachabilityMap[target] = isReachable;

                if (message.Length > 0)
                    message.Append(Environment.NewLine);

                message.Append($"Connection to '{target.Description}' {(isReachable ? "restored" : "lost")}.");
            }
            else
                _targetToReachabilityMap.Add(target, isReachable);
        }

        return message.ToString();
    }

    private static async Task<bool> IsReachable(Target target)
    {
        var attempt = 0;
        while (attempt++ < AttemptsWhenNotReachable)
        {
            var pingStatus = await PingAsync(target);
            if (pingStatus == IPStatus.Success)
                return true;

            await Task.Delay(BaseDelay * attempt);
        }

        return false;
    }

    private static async Task<IPStatus> PingAsync(Target target)
    {
        return target.Port.HasValue
            ? Ping(target.Host, target.Port.Value)
            : await PingAsync(target.Host);
    }

    private static async Task<IPStatus> PingAsync(string host)
    {
        try
        {
            using var ping = new System.Net.NetworkInformation.Ping();
            var pingReply = await ping.SendPingAsync(host).ConfigureAwait(false);

            return pingReply.Status;
        }
        catch
        {
            return IPStatus.Unknown;
        }
    }

    private static IPStatus Ping(string host, int port)
    {
        try
        {
            using var client = new TcpClient(host, port);
            return IPStatus.Success;
        }
        catch
        {
            return IPStatus.Unknown;
        }
    }

    public async Task<string> GetCurrentStateAsync(TimeSpan? timeout = null)
    {
        if (_settings.Targets.Length == 0)
            return string.Empty;

        var stateMessageBuilder = new StringBuilder();
        foreach (var target in _settings.Targets)
        {
            var hostStatus = await PingAsync(target).WaitAsync(timeout ?? TimeSpan.MaxValue)
                .ContinueWith(task =>
                {
                    if (!task.IsFaulted)
                        return task.Result;

                    if (task.Exception!.InnerExceptions.SingleOrDefault() is TimeoutException)
                        return IPStatus.TimedOut;

                    throw task.Exception;
                }, TaskContinuationOptions.None);
            var targetName = target.Description ?? target.Host + (target.Port.HasValue ? $":{target.Port}" : string.Empty);
            stateMessageBuilder.AppendLine($"{targetName}: {hostStatus}");
        }

        return stateMessageBuilder.ToString().Trim();
    }
}
