using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zs.Bot.Data.Models;
using Zs.Bot.Data.Queries;
using Zs.Common.Extensions;

namespace Zs.Bot.Data.Repositories;

public abstract class CommonRepository<TContext, TEntity> : IRepository<TEntity>
    where TEntity : DbEntity
    where TContext : DbContext
{
    private readonly ILogger<CommonRepository<TContext, TEntity>> _logger;
    protected IQueryFactory QueryFactory { get; }
    protected IDbContextFactory<TContext> ContextFactory { get; }

    protected CommonRepository(
        IDbContextFactory<TContext> contextFactory,
        IQueryFactory queryFactory,
        ILogger<CommonRepository<TContext, TEntity>> logger)
    {
        ContextFactory = contextFactory;
        QueryFactory = queryFactory;
        _logger = logger;
    }

    protected async Task<IReadOnlyList<TEntity>> FindByRawDataConditionAsync(ICondition condition, CancellationToken cancellationToken)
    {
        var tableName = await GetTableNameAsync(cancellationToken).ConfigureAwait(false);
        var findByRawDataIdSql = QueryFactory.CreateFindByConditionQuery(tableName, condition);

        LogTrace(condition, findByRawDataIdSql);

        return await FindAllBySqlAsync(findByRawDataIdSql, cancellationToken).ConfigureAwait(false);
    }

    private void LogTrace(ICondition condition, string sql, [CallerMemberName] string methodName = null)
    {
        _logger.LogTraceIfNeed("Entity: {Entity}, Method: {Method}, Condition: {Condition}, SQL: {SQL}", typeof(TEntity).Name, methodName, condition, sql);
    }

    private async Task<string> GetTableNameAsync(CancellationToken cancellationToken)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var entityType = typeof(TEntity);
        var schemaName = context.Model.FindEntityType(entityType)!.GetSchema()!;
        var tableName = context.Model.FindEntityType(entityType)!.GetTableName()!;
        var fullTableName = string.IsNullOrWhiteSpace(schemaName)
            ? tableName
            : $"{schemaName}.{tableName}";

        return fullTableName;
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var exists = await context.Set<TEntity>().AnyAsync(i => i.Id == id, cancellationToken).ConfigureAwait(false);

        return exists;
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var count = predicate != null
            ? await context.Set<TEntity>().CountAsync(predicate, cancellationToken).ConfigureAwait(false)
            : await context.Set<TEntity>().CountAsync(cancellationToken).ConfigureAwait(false);

        return count;
    }

    public virtual async Task<TEntity?> FindAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var query = context.Set<TEntity>();
        var entity = await query.AsNoTracking().FirstOrDefaultAsync(i => i.Id.Equals(id), cancellationToken).ConfigureAwait(false);

        return entity;
    }

    protected async Task<TEntity?> FindBySqlAsync(string sql, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var query = context.Set<TEntity>().FromSqlRaw(sql);
        var entity = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return entity;
    }

    internal async Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var query = context.Set<TEntity>().Where(predicate);
        if (orderBy != null)
            query = orderBy(query);

        var entity = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return entity;
    }

    internal async Task<IReadOnlyList<TEntity>> FindAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        IQueryable<TEntity> query = context.Set<TEntity>();

        if (predicate != null)
            query = query.Where(predicate);

        if (orderBy != null)
            query = orderBy(query);

        if (skip != null)
            query = query.Skip((int)skip);

        if (take != null)
            query = query.Take((int)take);

        var entities = await query.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

        return entities;
    }

    public async Task<IReadOnlyList<TEntity>> FindAllAsync(CancellationToken cancellationToken)
    {
        return await FindAllAsync(predicate: null, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TEntity>> FindAllAsync(long[] ids, CancellationToken cancellationToken = default)
    {
        return await FindAllAsync(i => ids.Contains(i.Id), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    protected async Task<IReadOnlyList<TEntity>> FindAllBySqlAsync(string sql, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(sql);

        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var query = context.Set<TEntity>().FromSqlRaw(sql);
        var entities = await query.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

        return entities;
    }

    public virtual async Task<int> AddAsync(TEntity item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        context.Set<TEntity>().Add(item);
        return await context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<int> AddRangeAsync(IReadOnlyCollection<TEntity> items, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);

        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        context.Set<TEntity>().AddRange(items);
        return await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<int> DeleteAsync(TEntity item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        context.Set<TEntity>().Remove(item);
        return await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<int> DeleteRangeAsync(IReadOnlyCollection<TEntity> items, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);

        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        context.Set<TEntity>().RemoveRange(items);
        return await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}