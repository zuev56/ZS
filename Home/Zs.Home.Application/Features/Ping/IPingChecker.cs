using System;
using System.Threading.Tasks;

namespace Zs.Home.Application.Features.Ping;

public interface IPingChecker
{
    Task<PingResult> PingAsync(Target target, TimeSpan? timeout = null);
    Task<PingResult> PingAsync(Target target, int attempts, TimeSpan timeoutForAttempt);
}
