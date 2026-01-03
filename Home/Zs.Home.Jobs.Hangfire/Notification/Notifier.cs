using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;
using Zs.Common.Services.Http;

namespace Zs.Home.Jobs.Hangfire.Notification;

public sealed class Notifier
{
    private readonly string _homeBotUrl;
    private readonly ILogger<Notifier> _logger;

    public Notifier(IConfiguration configuration, ILogger<Notifier> logger)
    {
        _homeBotUrl = configuration.GetRequiredSection("HomeBotUrl").Value!.TrimEnd('/');
        _logger = logger;
    }

    public async Task SendNotificationAsync(string message, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var url = $"{_homeBotUrl}/send";
        var notification = new Application.Models.Notification(message);
        try
        {
            await Request.Create(url)
                .WithLogger(_logger)
                .PostAsync(notification, ct);
        }
        catch
        {
            _logger.LogErrorIfNeed($"Unable to send notification: \"{message}\"");
        }
    }
}
