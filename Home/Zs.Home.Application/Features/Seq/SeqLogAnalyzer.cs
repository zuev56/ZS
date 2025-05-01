using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Seq.Api;
using Seq.Api.Model.Events;
using Zs.Common.Models;
using static System.Environment;
using static System.StringComparison;
using static Zs.Home.Application.Features.Seq.Constants;

namespace Zs.Home.Application.Features.Seq;

internal sealed class SeqLogAnalyzer : ILogAnalyzer
{
    private const string Unknown = nameof(Unknown);
    private readonly SeqSettings _settings;

    public SeqLogAnalyzer(IOptions<SeqSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<IReadOnlyList<LogEntry>> GetLogEntriesAsync(DateTimeRange dateTimeRange, CancellationToken ct = default)
    {
        var seq = new SeqConnection(_settings.Url, _settings.ApiKey);

        var events = await seq.Events.ListAsync(
                fromDateUtc: dateTimeRange.Start,
                toDateUtc: dateTimeRange.End,
                count: _settings.MaxEventsPerRequest,
                cancellationToken: ct)
            .ConfigureAwait(false);

        return events.Select(ToLogEntry).ToList();
    }

    public async Task<LogSummary> GetSummaryAsync(DateTimeRange dateTimeRange, CancellationToken ct = default)
    {
        var logEntries = await GetLogEntriesAsync(dateTimeRange, ct).ConfigureAwait(false);

        int EntriesCount(string logLevel) => logEntries.Count(e => e.Level.Equals(logLevel, InvariantCultureIgnoreCase));

        return new LogSummary
        {
            Count = logEntries.Count,
            LogLevelToCountMap = new Dictionary<LogLevel, int>
            {
                [LogLevel.Trace] = EntriesCount("VERBOSE"),
                [LogLevel.Debug] = EntriesCount("DEBUG"),
                [LogLevel.Information] = EntriesCount("INFORMATION"),
                [LogLevel.Warning] = EntriesCount("WARNING"),
                [LogLevel.Error] = EntriesCount("ERROR"),
                [LogLevel.Critical] = EntriesCount("FATAL")
            },

            ApplicationToCountMap = logEntries
                .GroupBy(e => e.ApplicationName)
                .ToDictionary(k => k.Key ?? Unknown, v => v.Count()),

            MessageInfos = logEntries
                .GroupBy(e => new { ApplicationName = e.ApplicationName ?? Unknown, e.MessagePattern })
                .Select(g =>
                {
                    var count = g.Count();
                    var last = g.MaxBy(e => e.Timestamp)!;
                    var message = (count > 1 ? "=[SIMILAR MESSAGE]= " : string.Empty)
                        + logEntries.Last(e => e.MessagePattern == g.Key.MessagePattern).Message;
                    return new LogMessageInfo(g.Key.ApplicationName, message, last.Level, count, last.Timestamp);
                })
                .ToList()
        };
    }

    private static LogEntry ToLogEntry(EventEntity seqEvent)
    {
        var messageParts = seqEvent.MessageTemplateTokens.Select(t =>
            {
                if (!HasOnlyOnePropertySet(t))
                    return ($"[Text: {t.Text}, RawText: {t.RawText}, PropertyName: {t.PropertyName}, FormattedValue: {t.FormattedValue}]", null);

                var propertyValue = seqEvent.Properties.SingleOrDefault(p => p.Name == t.PropertyName)?.Value?.ToString();
                var messagePatternPart = t.Text ?? "X";
                var messagePart = t.Text ?? propertyValue
                    + (!string.IsNullOrEmpty(t.RawText) ? $"{NewLine}{NewLine}[RawText: {t.RawText}]" : string.Empty)
                    + (!string.IsNullOrEmpty(t.FormattedValue) ? $"{NewLine}{NewLine}[FormattedValue: {t.FormattedValue}]" : string.Empty);

                return (messagePart, messagePatternPart);
            })
            .ToList();


        return new LogEntry
        {
            // seqEvent.Timestamp задан в UTC. При парсинге происходит преобразование UTC -> Local
            Timestamp = DateTime.Parse(seqEvent.Timestamp),
            Level = seqEvent.Level,
            ApplicationName = seqEvent.Properties.SingleOrDefault(p => p.Name == ApplicationName)?.Value.ToString(),
            Message = string.Join(' ', messageParts.Select(p => p.messagePart)),
            MessagePattern = string.Join(' ', messageParts.Select(p => p.messagePatternPart)),
        };
    }

    private static bool HasOnlyOnePropertySet(MessageTemplateTokenPart instance)
    {
        var setPropertiesCount = 0;

        if (!string.IsNullOrEmpty(instance.Text)) setPropertiesCount++;
        if (!string.IsNullOrEmpty(instance.PropertyName)) setPropertiesCount++;
        if (!string.IsNullOrEmpty(instance.RawText)) setPropertiesCount++;
        if (!string.IsNullOrEmpty(instance.FormattedValue)) setPropertiesCount++;

        return setPropertiesCount == 1;
    }
}
