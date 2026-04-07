using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Zs.Home.Application.Features.RabbitMq;
using static Zs.Home.Application.Features.RabbitMq.Constants;

namespace Zs.Home.Jobs.Hangfire.Notification;

public sealed class RabbitMqService
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqService> _logger;

    public RabbitMqService(ConnectionFactory connectionFactory, IOptions<RabbitMqSettings> settings, ILogger<RabbitMqService> logger)
    {
        _connectionFactory = connectionFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task PublishToNotificationsAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var jsonString = JsonSerializer.Serialize(message);

        await channel.BasicPublishAsync(
            exchange: _settings.MainExchange,
            routingKey: _settings.NotificationsKey,
            mandatory: true,
            body: Encoding.UTF8.GetBytes(jsonString),
            basicProperties: new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json"
            },
            cancellationToken: cancellationToken);
    }

    public async Task SetupRabbitMqAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _settings.MainExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _settings.DlxExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _settings.NotificationQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", _settings.DlxExchange },
                { "x-dead-letter-routing-key", _settings.DeadLettersKey },
                { "x-message-ttl", OneDayMs},
                { RetryCountHeader, 0}
            },
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _settings.DeadLettersQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _settings.NotificationQueue,
            exchange: _settings.MainExchange,
            routingKey: _settings.NotificationsKey,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _settings.DeadLettersQueue,
            exchange: _settings.DlxExchange,
            routingKey: _settings.DeadLettersKey,
            cancellationToken: cancellationToken);
    }
}
