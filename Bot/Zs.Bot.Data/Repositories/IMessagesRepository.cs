using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Data.Models;
using Zs.Common.Models;

namespace Zs.Bot.Data.Repositories;

public interface IMessagesRepository : IRepository<Message>
{
    Task<IReadOnlyList<Message>> FindByRawDatesAsync(long rawChatId, DateTimeRange rawDateTimeRange, CancellationToken cancellationToken = default);
    Task<Message?> FindByRawIdAsync(long rawMessageId, long rawChatId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Message>> FindDialogWithBotAsync(long rawChatId, long rawUserId, string botName, DateTimeRange rawDateTimeRange, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Message>> FindWithTextAsync(long rawChatId, string searchText, DateTimeRange rawDateTimeRange, CancellationToken cancellationToken = default);
}