using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Zs.Home.Application.Features.Ping;

public interface IPingChecker
{
    Task<IPStatus> PingAsync(Target target, TimeSpan? timeout = null);
    Task<IPStatus> PingAsync(Target target, int attempts, TimeSpan timeoutForAttempt);
}
