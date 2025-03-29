using System.Threading;
using System.Threading.Tasks;
using Zs.Common.Models;

namespace Zs.Home.Application.Features.Seq;

public interface ILogAnalyzer
{
    Task<LogSummary> GetSummaryAsync(DateTimeRange dateTimeRange, CancellationToken cancellationToken = default);
}
