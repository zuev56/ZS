using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Zs.Common.Extensions;

namespace Zs.Home.Application.Features.Ping;

public sealed record PingTarget
{
    public PingTarget(string ip, short? port = null)
    {
        Ip = IPAddress.Parse(ip);
        Port = port;
    }

    public IPAddress Ip { get; }
    public short? Port { get; }
}

internal sealed class PingChecker
{
    private static readonly TimeSpan BaseDelay = 300.Milliseconds();

    public async Task<IPStatus> PingAsync(string hostNameOrAddress)
    {
        try
        {
            using var ping = new System.Net.NetworkInformation.Ping();
            var pingReply = await ping.SendPingAsync(hostNameOrAddress).ConfigureAwait(false);

            return pingReply.Status;
        }
        catch
        {
            return IPStatus.Unknown;
        }
    }

    [Obsolete("Скорее всего, этот метод избыточен. Проверить вызов PingAsync(1.1.1.1:123)")]
    public static IPStatus Ping(string host, short port)
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

    // Если PingAsync(1.1.1.1:123) сработает, то PingTarget можно заменить на string
    public async Task<bool> IsReachable(PingTarget target, int attempts = 1)
    {
        var attempt = 0;
        while (attempt++ < attempts)
        {
            var pingStatus = await PingAsync(target);
            if (pingStatus == IPStatus.Success)
                return true;

            await Task.Delay(BaseDelay * attempt);
        }

        return false;
    }

    private async Task<IPStatus> PingAsync(PingTarget target)
    {
        return target.Port.HasValue
            ? Ping(target.Ip.ToString(), target.Port.Value)
            : await PingAsync(target.Ip.ToString());
    }
}
