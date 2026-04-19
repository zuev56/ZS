using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Zs.Common.Extensions;
using Zs.Home.Application.Features.RabbitMq;
using Zs.Home.Bot.Interaction;
using static Zs.Home.Application.Features.RabbitMq.Constants;

namespace Zs.Home.Bot.RabbitMq;

internal sealed class RabbitMqListener : IAsyncDisposable
{
    private readonly Notifier _notifier;
    private readonly ConnectionFactory _connectionFactory;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqListener> _logger;

    private IConnection _connection;
    private IChannel _channel;
    private bool _disposed;

    public RabbitMqListener(Notifier notifier, ConnectionFactory connectionFactory, IOptions<RabbitMqSettings> settings, ILogger<RabbitMqListener> logger)
    {
        _notifier = notifier;
        _connectionFactory = connectionFactory;
        _logger = logger;
        _settings = settings.Value;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        var retryCount = 0;
        while (true)
        {
            try
            {
                _connection = await _connectionFactory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(
                    exchange: _settings.MainExchange,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += ReceivedNotificationAsync;

                await _channel.BasicConsumeAsync(_settings.NotificationQueue, autoAck: false, consumer);
                break;
            }
            catch (Exception e)
            {
                _logger.LogErrorIfNeed(e, $"{nameof(RabbitMqListener)}, initialization error. retryCount: {++retryCount}");
                await Task.Delay(10.Seconds());
            }
        }

    }

    private async Task ReceivedNotificationAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        string? body = null;
        try
        {
            body = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            await _notifier.NotifyAsync(body);

            await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
        }
        catch (Exception e)
        {
            var retryCount = eventArgs.BasicProperties.Headers?.TryGetValue(RetryCountHeader, out var value) == true
                ? Convert.ToInt32(value)
                : 0;

            _logger.LogErrorIfNeed(e, $"ReceivedNotificationAsync: {body}, retryCount: {retryCount}");

            if (retryCount < _settings.MaxRetryCount)
            {
                var properties = new BasicProperties
                {
                    Headers = new Dictionary<string, object?> { [RetryCountHeader] = retryCount + 1 },
                    Persistent = eventArgs.BasicProperties.Persistent,
                    ContentType = eventArgs.BasicProperties.ContentType
                };

                await _channel.BasicPublishAsync(
                    exchange: _settings.MainExchange,
                    routingKey: eventArgs.RoutingKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: eventArgs.Body);

                await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
            }
            else
            {
                await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await _connection.CloseAsync();
        await _connection.DisposeAsync();
        await _channel.CloseAsync();
        await _channel.DisposeAsync();

        _disposed = true;
    }
}
