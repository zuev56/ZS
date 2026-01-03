using Zs.Home.WebApi.Features.Ping.Models;

namespace Zs.Home.WebApi.Features.Ping;

public interface IPingChecker
{
    Task<PingResult> PingAsync(Target target, TimeSpan? timeout = null);
    Task<PingResult> PingAsync(Target target, int attempts, TimeSpan timeoutForAttempt);
}
