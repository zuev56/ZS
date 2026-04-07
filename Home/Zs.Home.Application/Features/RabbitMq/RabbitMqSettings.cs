using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Application.Features.RabbitMq;

public sealed class RabbitMqSettings
{
    internal const string SectionName = "RabbitMq";

    [Required]
    public string HostName { get; set; } = null!;
    [Required]
    public int Port { get; set; }
    [Required]
    public string UserName { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;

    [Required]
    public string MainExchange { get; set; } = null!;
    [Required]
    public string DlxExchange { get; set; } = null!;
    [Required]
    public string NotificationQueue { get; set; } = null!;
    [Required]
    public string DeadLettersQueue { get; set; } = null!;
    [Required]
    public string NotificationsKey { get; set; } = null!;
    [Required]
    public string DeadLettersKey { get; set; } = null!;

    public int MaxRetryCount { get; set; } = 3;
}
