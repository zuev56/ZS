using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Data.Models;

namespace Zs.Bot.Data.Repositories;

public interface IRepository<TEntity>
    where TEntity : DbEntity
{
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task<TEntity?> FindAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> FindAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> FindAllAsync(long[] ids, CancellationToken cancellationToken = default);
    Task<int> AddAsync(TEntity item, CancellationToken cancellationToken = default);
    Task<int> AddRangeAsync(IReadOnlyCollection<TEntity> items, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(TEntity item, CancellationToken cancellationToken = default);
    Task<int> DeleteRangeAsync(IReadOnlyCollection<TEntity> items, CancellationToken cancellationToken = default);
}