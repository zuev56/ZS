using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Application.Features.Ping;

public sealed record Target
{
    [Required]
    public required string Host { get; init; } = null!;

    public short? Port { get; init; }

    public string? Description { get; init; }

    // Некорректное название, ведь порта может и не быть
    public string Socket => Host + (Port.HasValue ? $":{Port}" : string.Empty);

    public override string ToString()
        => (Port.HasValue ? $"{Host}:{Port}" : $"{Host}")
           + (!string.IsNullOrWhiteSpace(Description) ? $" ({Description})" : string.Empty);
}
