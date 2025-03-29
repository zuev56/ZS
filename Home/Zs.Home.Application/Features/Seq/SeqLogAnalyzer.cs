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
    private readonly SeqSettings _settings;

    public SeqLogAnalyzer(IOptions<SeqSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<LogSummary> GetSummaryAsync(DateTimeRange dateTimeRange, CancellationToken ct = default)
    {
        var seq = new SeqConnection(_settings.Url, _settings.ApiKey);

        var events = await seq.Events.ListAsync(
                fromDateUtc: dateTimeRange.Start,
                toDateUtc: dateTimeRange.End,
                count: _settings.MaxEventsPerRequest,
                cancellationToken: ct)
            .ConfigureAwait(false);

        var logEntries = events.Select(ToLogEntry).ToList();

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
                .ToDictionary(k => k.Key ?? "Unknown", v => v.Count()),

            // TODO: Научиться группировать сообщения, где различаются только параметры, а остальной текст общий
            MessageInfos = logEntries
                .GroupBy(e => e.Message)
                .Select(g =>
                {
                    var last = g.MaxBy(e => e.Timestamp)!;
                    return new LogMessageInfo(g.Key, last.Level, g.Count(), last.Timestamp);
                })
                .ToList()
        };
    }

    private static LogEntry ToLogEntry(EventEntity seqEvent)
    {
        var messageParts = seqEvent.MessageTemplateTokens.Select(t =>
        {
            if (!HasOnlyOnePropertySet(t))
                return $"[Text: {t.Text}, RawText: {t.RawText}, PropertyName: {t.PropertyName}, FormattedValue: {t.FormattedValue}]";

            var propertyValue = seqEvent.Properties.SingleOrDefault(p => p.Name == t.PropertyName)?.Value?.ToString() ?? "null";
            return t.Text ?? propertyValue
                + (!string.IsNullOrEmpty(t.RawText) ? $"{NewLine}{NewLine}[RawText: {t.RawText}]" : string.Empty)
                + (!string.IsNullOrEmpty(t.FormattedValue) ? $"{NewLine}{NewLine}[FormattedValue: {t.FormattedValue}]" : string.Empty);
        });

        return new LogEntry
        {
            // seqEvent.Timestamp задан в UTC. При парсинге происходит преобразование UTC -> Local
            Timestamp = DateTime.Parse(seqEvent.Timestamp),
            Level = seqEvent.Level,
            ApplicationName = seqEvent.Properties.SingleOrDefault(p => p.Name == ApplicationName)?.Value.ToString(),
            Message = string.Join(' ', messageParts)
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
