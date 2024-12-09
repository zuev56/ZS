using System;
using System.Threading.Tasks;

namespace Zs.Home.Application.Features.Hardware;

public interface IHasCurrentState
{
    Task<string> GetCurrentStateAsync(TimeSpan? timeout = null);
}
