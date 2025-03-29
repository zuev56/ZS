using System.ComponentModel.DataAnnotations;

namespace Zs.Home.Jobs.Hangfire.LogAnalyzer;

public sealed class LogAnalyzerSettings : Zs.Home.Application.Features.Seq.SeqSettings
{
    [Required]
    public required string CronExpression { get; set; }
}
