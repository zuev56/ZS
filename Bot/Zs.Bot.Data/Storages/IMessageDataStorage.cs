using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Data.Models;

namespace Zs.Bot.Data.Storages;

public interface IMessageDataStorage
{
    // TODO: DeleteMessage() // logically in DB
    Task SaveNewMessageDataAsync(MessageActionData messageActionData, CancellationToken cancellationToken);
    Task EditSavedMessageAsync(Message message, CancellationToken cancellationToken);
}
