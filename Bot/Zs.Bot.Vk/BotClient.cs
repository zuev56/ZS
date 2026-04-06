using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using VkNet;
using VkNet.Extensions.Polling;
using VkNet.Extensions.Polling.Models.Configuration;
using VkNet.Extensions.Polling.Models.Update;
using VkNet.Model.GroupUpdate;
using VkNet.Model.RequestParams;
using Zs.Bot.Data.Models;
using Zs.Bot.Services.Messaging;
using Zs.Bot.Services.Pipeline;
using Zs.Common.Extensions;
using DbChat = Zs.Bot.Data.Models.Chat;
using DbMessage = Zs.Bot.Data.Models.Message;
using DbUser = Zs.Bot.Data.Models.User;

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

        InitializeVkApiClient();
    }

    private void InitializeVkApiClient()
    {
        var cancellationTokenSource = new CancellationTokenSource();

        // if (_vkApiClient.IsAuthorizedAsUser())
        // {
        //     var userLongPoll = _vkApiClient.StartUserLongPollAsync(UserLongPollConfiguration.Default, cancellationTokenSource.Token);
        //
        //     _ = StartReceivingAsync(userLongPoll.AsChannelReader(), HandleUserUpdate);
        // }
        // else
        if(_vkApiClient.IsAuthorizedAsGroup())
        {
            var groupLongPoll = _vkApiClient.StartGroupLongPollAsync(GroupLongPollConfiguration.Default, cancellationTokenSource.Token);

            //_ = StartReceivingAsync(groupLongPoll.AsChannelReader(), u => HandleGroupUpdate(u));
        }
        else
            _logger.LogInformationIfNeed("Для корректной работы приложения требуется авторизация.");
    }

    private void HandleUserUpdate(UserUpdate userUpdate)
    {
        var vkMessage = userUpdate.Message;
        var vkUser = userUpdate.Sender.User;

        _logger.LogInformationIfNeed($"Получен пользовательский апдейт: {vkMessage.Id}. Текст: {vkMessage.Text}..");

        // TODO: сходить в БД. Но как отсюда? Может сделать заглушки, где будет корректным только идентификатор?
        //       Или взять данные из VkNet.Model.User
        var dbMessage = new DbMessage
        {
            Id = vkMessage.Id!.Value,
            ChatId = vkMessage.ChatId!.Value,
            Chat = new DbChat
            {
                Id = vkMessage.ChatId!.Value,
                Name = "", // TODO
                RawData = "" // TODO
            },
            UserId = vkMessage.UserId!.Value,
            User = new DbUser
            {
                Id = vkUser.Id,
                UserName = vkUser.ScreenName,
                FullName = $"{vkUser.FirstName} {vkUser.LastName}".Trim().ReplaceEmojiWithX(),
                RawData = Serialize(vkUser)
            },
            ReplyToMessageId = vkMessage.ReplyMessage?.Id,
            RawData = vkMessage.Body
        };

        _ = StartMessagePipelineAsync(dbMessage, MessageAction.Received);
    }

    private void HandleGroupUpdate(GroupUpdate update)
    {
        _logger.LogInformationIfNeed($"Получен групповой апдейт для группы {update}. Тип: {update.Type}.");
        throw new  NotImplementedException();
    }


    private static async Task StartReceivingAsync<TUpdate>(ChannelReader<TUpdate> channelReader, Action<TUpdate> updateAction)
    {
        await foreach (TUpdate update in channelReader.ReadAllAsync())
            updateAction(update);
    }

    private async Task StartMessagePipelineAsync(DbMessage dbMessage, MessageAction messageAction)
    {
        var messageActionData = CreateMessageActionData(dbMessage, messageAction);

        try
        {
            if (_firstMessagePipelineStep != null)
                await _firstMessagePipelineStep.PerformAsync(messageActionData, CancellationToken.None);
        }
        catch (Exception e)
        {
            _logger.LogErrorIfNeed(e, "Pipeline failed for message {DbMessageId}", dbMessage.Id);
        }
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

    private static MessageActionData CreateMessageActionData(DbMessage dbMessage, MessageAction messageAction)
    {
        return new MessageActionData
        {
            Message = dbMessage,
            Chat = dbMessage.Chat,
            User = dbMessage.User,
            Action = messageAction
        };
    }

    public async Task<DbMessage> SendMessageAsync(string text, DbChat dbChat, DbMessage? messageToReply, CancellationToken cancellationToken = default)
    {
        var messagesSendParams = new MessagesSendParams
        {
            ChatId = dbChat.Id,
            Message = PrepareMessageText(text),
            ReplyTo = messageToReply?.Id
        };

        var messageId = await _vkApiClient.Messages.SendAsync(messagesSendParams);//, cancellationToken);

        var dbMessage = new DbMessage
        {
            Id = messageId,
            ChatId = messagesSendParams.ChatId.Value,
            Chat = new DbChat
            {
                // TODO
            },
            UserId = _vkApiClient.UserId!.Value,
            User = new DbUser
            {
                // TODO
            },
            ReplyToMessageId = messagesSendParams.ReplyTo,
            RawData = "" // Т.к. SendAsync возвращает только Id, то сюда добавить нечего
        };

        _ = StartMessagePipelineAsync(dbMessage, MessageAction.Sent);

        return dbMessage;
    }

    private static string PrepareMessageText(string text)
    {
        if (text.Length > MaxMessageLength)
            return text[..(MaxMessageLength - 3)] + "...";

        return text;
    }

    public async Task DeleteMessageAsync(DbMessage message, CancellationToken cancellationToken = default)
    {
        await _vkApiClient.Messages.DeleteAsync([(ulong)message.Id], spam: false, deleteForAll: true);//, token: cancellationToken);

        await StartMessagePipelineAsync(message, MessageAction.Deleted);
    }

    public async Task<string> GetBotInfoAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
        // var bot = await _vkApiClient.GetMeAsync(cancellationToken).ConfigureAwait(false);
        // var json = JsonSerializer.Serialize(bot);
        // _logger.LogDebugIfNeed("Get bot info: {Bot}", json);
        //
        // return json;
    }

    private static string Serialize<TItem>(TItem item)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        };

        return JsonSerializer.Serialize(item, jsonSerializerOptions).ReplaceEmojiWithX()!;
    }
}
