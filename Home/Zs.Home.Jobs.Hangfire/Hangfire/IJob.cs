using System.Threading;
using System.Threading.Tasks;

namespace Zs.Home.Jobs.Hangfire.Hangfire;

public interface IJob
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
