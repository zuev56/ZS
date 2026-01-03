using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zs.Common.Models;

namespace Zs.Home.Application.Features.Seq;

public interface ILogAnalyzer
{
    Task<IReadOnlyList<LogEntry>> GetLogEntriesAsync(DateTimeRange dateTimeRange, CancellationToken cancellationToken = default);
    Task<LogSummary> GetSummaryAsync(DateTimeRange dateTimeRange, CancellationToken cancellationToken = default);
}
