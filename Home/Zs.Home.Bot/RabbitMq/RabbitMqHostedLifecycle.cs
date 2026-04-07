using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Zs.Home.Bot.RabbitMq;

internal sealed class RabbitMqHostedLifecycle : IHostedService
{
    private readonly RabbitMqListener _rabbitMqListener;
    private readonly ILogger<RabbitMqHostedLifecycle> _logger;

    public RabbitMqHostedLifecycle(RabbitMqListener rabbitMqListener, ILogger<RabbitMqHostedLifecycle> logger)
    {
        _rabbitMqListener = rabbitMqListener;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting RabbitMQ Listener");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ Listener");
        return _rabbitMqListener.DisposeAsync().AsTask();
    }
}
