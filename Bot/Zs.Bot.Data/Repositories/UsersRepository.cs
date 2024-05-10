using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zs.Bot.Data.Models;
using Zs.Bot.Data.Queries;

namespace Zs.Bot.Data.Repositories;

public class UsersRepository<TContext> : CommonRepository<TContext, User>, IUsersRepository
    where TContext : DbContext
{
    public UsersRepository(IDbContextFactory<TContext> contextFactory, IQueryFactory queryFactory, ILogger<UsersRepository<TContext>> logger)
        : base(contextFactory, queryFactory, logger)
    {
    }

    public async Task<User?> FindByRawIdAsync(long rawId, CancellationToken cancellationToken)
    {
        var userIdPath = QueryFactory.RawDataStructure.User.Id;
        var condition = RawData.Eq(userIdPath, rawId);
        var users = await FindByRawDataConditionAsync(condition, cancellationToken).ConfigureAwait(false);

        return users.SingleOrDefault();
    }

    public async Task<IReadOnlyList<User>> FindByRolesAsync(IEnumerable<Role> userRoles, CancellationToken cancellationToken)
    {
        return await FindAllAsync(u => userRoles.Contains(u.Role), cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}