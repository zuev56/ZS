using MediatR;
using Microsoft.Extensions.Options;
using Zs.Home.Application.Features.Ping;

namespace Zs.Home.WebApi.Features.Ping;

public sealed class PingHandler : IRequestHandler<PingRequest, PingResponse>
{
    private readonly IPingChecker _pingChecker;
    private readonly PingCheckerSettings _settings;

    public PingHandler(IPingChecker pingChecker, IOptions<PingCheckerSettings> options)
    {
        _pingChecker = pingChecker;
        _settings = options.Value;
    }

    public async Task<PingResponse> Handle(PingRequest request, CancellationToken cancellationToken)
    {
        var targets = request.Target != null ? [request.Target] : _settings.Targets;
        var pingTasks = targets
            .Select(target =>_pingChecker.PingAsync(target, _settings.Attempts, _settings.Timeout))
            .ToList();

        await Task.WhenAll(pingTasks);

        return new PingResponse(pingTasks.Select(t => t.Result).ToList());
    }
}
