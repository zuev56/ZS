using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;

namespace Zs.Home.Application.Features.Ping;

internal sealed class PingChecker : IPingChecker
{
    private readonly ILogger<PingChecker> _logger;

    public PingChecker(ILogger<PingChecker> logger)
    {
        _logger = logger;
    }

    public async Task<IPStatus> PingAsync(Target target, TimeSpan? timeout)
    {
        var sw = Stopwatch.StartNew();

        if (target.Port.HasValue)
        {
            var pingTask = Task.Run(() => Ping(target.Host, target.Port.Value));
            await Task.WhenAny(pingTask, Task.Delay(timeout ?? TimeSpan.Zero)).ConfigureAwait(false);

            var resultForLog = pingTask.IsFaulted ? pingTask.Exception.GetType().Name : pingTask.IsCompleted ? pingTask.Result.ToString() : "Timeout";
            _logger.LogTraceIfNeed("Ping {Target}. Result: {Result}, Elapsed: {Elapsed} ms]",
                target, resultForLog, sw.ElapsedMilliseconds);

            return pingTask switch
            {
                {IsCompletedSuccessfully: true} => pingTask.Result,
                {IsCompleted: false, IsFaulted: false} => IPStatus.TimedOut,
                _ => IPStatus.Unknown
            };
        }

        var result = await PingAsync(target.Host, timeout).ConfigureAwait(false);

        _logger.LogTraceIfNeed("Ping {Target}. Result: {Result}, Elapsed: {Elapsed} ms]", target, result, sw.ElapsedMilliseconds);

        return result;
    }

    public async Task<IPStatus> PingAsync(Target target, int attempts, TimeSpan timeoutForAttempt)
    {
        var sw = Stopwatch.StartNew();
        var attempt = 0;
        var pingStatus = IPStatus.Unknown;
        while (attempt++ < attempts)
        {
            pingStatus = await PingAsync(target, timeoutForAttempt).ConfigureAwait(false);
            if (pingStatus == IPStatus.Success)
                break;

            await Task.Delay(timeoutForAttempt * attempt);
        }

        _logger.LogTraceIfNeed("Ping {Target}. Attempts: {Attempts}, Elapsed: {Elapsed} ms]", target,  attempt, sw.ElapsedMilliseconds);

        return pingStatus;
    }

    private static async Task<IPStatus> PingAsync(string host, TimeSpan? timeout)
    {
        try
        {
            timeout ??= TimeSpan.MaxValue;
            using var ping = new System.Net.NetworkInformation.Ping();
            var pingReply = await ping.SendPingAsync(host, timeout.Value).ConfigureAwait(false);

            return pingReply.Status;
        }
        catch
        {
            return IPStatus.Unknown;
        }
    }

    private static IPStatus Ping(string host, short port)
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
}
