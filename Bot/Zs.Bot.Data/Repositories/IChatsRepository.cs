using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Data.Models;

namespace Zs.Bot.Data.Repositories;

public interface IChatsRepository : IRepository<Chat>
{
    Task<Chat?> FindByRawIdAsync(long rawId, CancellationToken cancellationToken = default);
}