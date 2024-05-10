using System.Threading;
using System.Threading.Tasks;

namespace Zs.Common.Abstractions;

public interface IDbClient
{
    Task<string?> GetQueryResultAsync(string query, CancellationToken cancellationToken = default);
}