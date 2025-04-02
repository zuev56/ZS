using System.Threading;
using System.Threading.Tasks;

namespace Zs.Home.Application.Features.Hardware;

public interface IHardwareMonitor
{
    Task<HardwareStatus> GetHardwareStatusAsync(CancellationToken cancellationToken = default);
}
