using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Home.Application.Features.Seq;
using Zs.Home.Jobs.Hangfire.Extensions;
using Zs.Home.Jobs.Hangfire.Notification;

namespace Zs.Home.Jobs.Hangfire.LogAnalyzer;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class LogAnalyzerJob
{
    private readonly ILogAnalyzer _logAnalyzer;
    private readonly Notifier _notifier;
    private readonly ILogger<LogAnalyzerJob> _logger;

    public LogAnalyzerJob(
        ILogAnalyzer logAnalyzer,
        Notifier notifier,
        ILogger<LogAnalyzerJob> logger)
    {
        _logAnalyzer = logAnalyzer;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogJobStart();

        // TODO: DateTime.UtcNow.AddDays(-?)
        var dateTimeRange = new DateTimeRange(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        var logSummary = await _logAnalyzer.GetSummaryAsync(dateTimeRange, ct);

        var notification = CreateNotification(logSummary);

        await _notifier.SendNotificationAsync(notification, ct);

        _logger.LogJobFinish(sw.Elapsed);
    }

    private static string CreateNotification(LogSummary logSummary)
    {
        var messages = logSummary.MessageInfos
            .OrderByDescending(m => m.LastTimestamp)
            .Select(m => $"[{m.LastTimestamp.ToLocalTime():u} {GetShortLogLevel(m.Level)}]" +
                         $" {m.Message.ReplaceEndingWithThreeDots(maxStringLength: 200)}" +
                         $" {(m.Count > 1 ? $"({m.Count} times)" : "")}");

        var levelToCount = logSummary.LogLevelToCountMap
            .Select(m => m.Value > 0 ? $"{m.Key}s: {m.Value}" : string.Empty)
            .Where(m => !string.IsNullOrWhiteSpace(m));

        var appToCount = logSummary.ApplicationToCountMap
            .Select(m => $"{m.Key}: {m.Value}")
            .Where(m => !string.IsNullOrWhiteSpace(m));

        var line = Environment.NewLine;
        return $"LOG SUMMARY{line}{line}" +
               $"All: {logSummary.Count}{line}" +
               $"{string.Join(line, levelToCount)}{line}{line}" +
               $"Applications:{line}" +
               $"{string.Join(line, appToCount)}{line}{line}" +
               $"Messages:{line}" +
               $"{string.Join($"{line}{line}", messages)}"
                   .ReplaceEndingWithThreeDots(maxStringLength: 2000);
    }

    private static string GetShortLogLevel(string logLevel)
        => logLevel.ToUpper() switch
        {
            "VERBOSE" => "VRB",
            "DEBUG" => "DBG",
            "INFORMATION" => "INF",
            "WARNING" => "WRN",
            "ERROR" => "ERR",
            _ => logLevel.ToUpper()
        };
}
