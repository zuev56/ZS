using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAdmin.Bot.Abstractions;
using ChatAdmin.Bot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zs.Bot.Data.Repositories;

namespace ChatAdmin.Bot.Repositories;

internal sealed class BansRepository<TContext> : CommonRepository<TContext, Ban, int>, IBansRepository
    where TContext : DbContext
{
    public BansRepository(IDbContextFactory<TContext> contextFactory,
        TimeSpan? criticalQueryExecutionTimeForLogging = null,
        ILogger<CommonRepository<TContext, Ban, int>> logger = null)
        : base(contextFactory, criticalQueryExecutionTimeForLogging, logger)
    {
    }

    public async Task<List<Ban>> FindAllTodaysBanWarningsAsync()
    {
        return await FindAllAsync(b => b.InsertDate > DateTime.UtcNow.Date && b.FinishDate == null).ConfigureAwait(false);
    }
}
