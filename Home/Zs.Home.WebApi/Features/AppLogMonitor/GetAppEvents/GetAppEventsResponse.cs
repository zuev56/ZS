using Zs.Home.Application.Features.Seq;

namespace Zs.Home.WebApi.Features.AppLogMonitor.GetAppEvents;

public sealed record GetAppEventsResponse(IReadOnlyList<LogEntry> LogEntries);
