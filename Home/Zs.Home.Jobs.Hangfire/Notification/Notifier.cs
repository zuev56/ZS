using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;
using Zs.Home.Application.Features.RabbitMq;

namespace Zs.Home.Jobs.Hangfire.Notification;

public sealed class Notifier
{
    private readonly RabbitMqService _rabbitMqService;
    private readonly ILogger<Notifier> _logger;

    public Notifier(RabbitMqService rabbitMqService, ILogger<Notifier> logger)
    {
        _rabbitMqService = rabbitMqService;
        _logger = logger;
    }

    public async Task SendNotificationAsync(string message, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        try
        {
            await _rabbitMqService.PublishToNotificationsAsync(message, ct);
        }
        catch (Exception e)
        {
            _logger.LogErrorIfNeed(e, "An error occurred while sending notification");
            throw;
        }
    }
}
