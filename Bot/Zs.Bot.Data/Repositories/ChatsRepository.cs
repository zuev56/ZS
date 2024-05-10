using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zs.Bot.Data.Models;
using Zs.Bot.Data.Queries;

namespace Zs.Bot.Data.Repositories;

public class ChatsRepository<TContext> : CommonRepository<TContext, Chat>, IChatsRepository
    where TContext : DbContext
{
    public ChatsRepository(IDbContextFactory<TContext> contextFactory, IQueryFactory queryFactory, ILogger<ChatsRepository<TContext>> logger)
        : base(contextFactory, queryFactory, logger)
    {
    }

    public async Task<Chat?> FindByRawIdAsync(long rawId, CancellationToken cancellationToken)
    {
        var chatIdPath = QueryFactory.RawDataStructure.Chat.Id;
        var condition = RawData.Eq(chatIdPath, rawId);
        var chats = await FindByRawDataConditionAsync(condition, cancellationToken).ConfigureAwait(false);

        return chats.SingleOrDefault();
    }
}