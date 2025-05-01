using System.ComponentModel.DataAnnotations;

namespace Zs.Home.WebApi.Client.Bootstrap;

public sealed class HomeClientSettings
{
    public const string SectionName = "HomeClient";

    [Required]
    public required string Url { get; init; }
}
