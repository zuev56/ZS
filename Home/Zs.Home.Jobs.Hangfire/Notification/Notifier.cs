using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Zs.Common.Services.Http;

namespace Zs.Home.Jobs.Hangfire.Notification;

public sealed class Notifier
{
    private readonly string _homeBotUrl;

    public Notifier(IConfiguration configuration)
    {
        _homeBotUrl = configuration.GetRequiredSection("HomeBotUrl").Value!.TrimEnd('/');
    }

    public Task SendNotificationAsync(string message, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(message))
            return Task.CompletedTask;

        var url = $"{_homeBotUrl}/send";
        var notification = new Application.Models.Notification(message);
        return Request.Create(url).PostAsync(notification, ct);
    }
}
