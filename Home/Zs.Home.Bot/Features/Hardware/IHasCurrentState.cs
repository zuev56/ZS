using System.Threading.Tasks;

namespace Zs.Home.Bot.Features.Hardware;

public interface IHasCurrentState
{
    Task<string> GetCurrentStateAsync();
}