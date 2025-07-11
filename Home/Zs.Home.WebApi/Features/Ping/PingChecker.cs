using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Zs.Common.Extensions;
using Zs.Home.WebApi.Features.Ping.Models;

namespace Zs.Home.WebApi.Features.Ping;

internal sealed class PingChecker : IPingChecker
{
    private readonly ILogger<PingChecker> _logger;

    public PingChecker(ILogger<PingChecker> logger)
    {
        _logger = logger;
    }

    public async Task<PingResult> PingAsync(Target target, TimeSpan? timeout)
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
                {IsCompletedSuccessfully: true} => CreatePingResult(target, pingTask.Result, sw.Elapsed),
                {IsCompleted: false, IsFaulted: false} => CreatePingResult(target, IPStatus.TimedOut, sw.Elapsed),
                _ => CreatePingResult(target, IPStatus.Unknown, sw.Elapsed)
            };
        }

        var result = await PingAsync(target.Host, timeout).ConfigureAwait(false);

        _logger.LogTraceIfNeed("Ping {Target}. Result: {Result}, Elapsed: {Elapsed} ms]", target, result, sw.ElapsedMilliseconds);

        return CreatePingResult(target, result, sw.Elapsed);
    }

    private static PingResult CreatePingResult(Target target, IPStatus status, TimeSpan? time)
        => new() { Host = target.Host, Port = target.Port, Description = target.Description, Status = status, Time = time };

    public async Task<PingResult> PingAsync(Target target, int attempts, TimeSpan timeoutForAttempt)
    {
        var sw = Stopwatch.StartNew();
        var attempt = 0;
        var pingResult = new PingResult { Host = target.Host, Port = target.Port, Description = target.Description, Status = IPStatus.Unknown };
        while (attempt++ < attempts)
        {
            pingResult = await PingAsync(target, timeoutForAttempt).ConfigureAwait(false);
            if (pingResult.Status == IPStatus.Success)
                break;

            await Task.Delay(timeoutForAttempt * attempt);
        }

        _logger.LogTraceIfNeed("Ping {Target}. Attempts: {Attempts}, Elapsed: {Elapsed} ms]", target,  attempt, sw.ElapsedMilliseconds);

        return pingResult;
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
