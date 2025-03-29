using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Application.Features.Seq;

public class SeqSettings
{
    public const string SectionName = "Seq";

    [Required]
    public string Url { get; init; } = null!;

    [Required]
    public string ApiKey { get; init; } = null!;

    [Required]
    public int MaxEventsPerRequest { get; init; }
}
