using Zs.Bot.Data.Models;
using Zs.Bot.Services.Messaging;
using Zs.Bot.Services.Pipeline;
using Zs.Common.Models;

namespace Zs.DemoBot;

internal sealed class AuthorizationStep : PipelineStep
{
    private readonly IBotClient _botClient;
    private readonly IReadOnlyList<long> _privilegedUserIds;

    public AuthorizationStep(IBotClient botClient, IReadOnlyList<long> privilegedUserIds)
    {
        _botClient = botClient;
        _privilegedUserIds = privilegedUserIds;
    }

    protected override async Task<Result> PerformInternalAsync(MessageActionData messageActionData, CancellationToken cancellationToken)
    {
        var (message, chat, user, action) = messageActionData;
        if (action != MessageAction.Received)
            return Result.Success();

        var userId = user?.Id;
        if (userId.HasValue && _privilegedUserIds.Contains(userId.Value))
            return Result.Success();

        var fault = Faults.Unauthorized;
        await _botClient.SendMessageAsync(fault.Code, chat!, messageToReply: message, cancellationToken);
        return fault;
    }
}