using Microsoft.Extensions.Logging;
using VkNet;
using VkNet.Model.RequestParams;
using Zs.Bot.Services.Messaging;
using Zs.Bot.Services.Pipeline;
using DbChat = Zs.Bot.Data.Models.Chat;
using DbMessage = Zs.Bot.Data.Models.Message;

namespace Zs.Bot.Vk;

public sealed class BotClient : IBotClient
{
    private const int MaxMessageLength = 4096;
    private PipelineStep? _firstMessagePipelineStep;
    private readonly VkApi _vkApiClient;
    private readonly ILogger _logger;

    public BotClient(VkApi vkApiClient, ILogger<BotClient> logger)
    {
        _vkApiClient = vkApiClient;
        _logger = logger;
    }

    public void AddToMessagePipeline(PipelineStep nextStep, CancellationToken cancellationToken)
    {
        if (_firstMessagePipelineStep == null)
        {
            _firstMessagePipelineStep = nextStep;
        }
        else
        {
            var lastStep = _firstMessagePipelineStep.GetLastStep();
            lastStep.Next = nextStep;
        }
    }

    public Task<DbMessage> SendMessageAsync(string text, DbChat dbChat, DbMessage? messageToReply, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public async Task<DbMessage> SendMessageAsync(string text, long userId)
    {
        var messagesSendParams = new MessagesSendParams
        {
            UserId = userId,
            Message = PrepareMessageText(text),
            RandomId = Random.Shared.NextInt64(long.MaxValue)
        };

        await _vkApiClient.Messages.SendAsync(messagesSendParams);

        return null!;
    }

    private static string PrepareMessageText(string text)
    {
        if (text.Length > MaxMessageLength)
            return text[..(MaxMessageLength - 3)] + "...";

        return text;
    }

    public Task DeleteMessageAsync(DbMessage message, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<string> GetBotInfoAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
