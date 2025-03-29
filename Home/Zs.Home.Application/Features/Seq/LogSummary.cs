using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Zs.Home.Application.Features.Seq;

public sealed record LogSummary
{
    public required int Count { get; init; }
    public required Dictionary<LogLevel, int> LogLevelToCountMap { get; init; }
    public required Dictionary<string, int> ApplicationToCountMap { get; init; }

    public required IReadOnlyList<LogMessageInfo> MessageInfos { get; init; }
}

public sealed record LogMessageInfo(string Message, string Level, int Count, DateTime LastTimestamp);
