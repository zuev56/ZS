using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Data.Models;

namespace Zs.Bot.Data.Repositories;

public interface IUsersRepository : IRepository<User>
{
    Task<User?> FindByRawIdAsync(long rawId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> FindByRolesAsync(IEnumerable<Role> userRoles, CancellationToken cancellationToken = default);
}