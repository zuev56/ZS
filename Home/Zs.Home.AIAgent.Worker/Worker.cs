using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Zs.Home.AIAgent.Worker.Extensions;
using Zs.Home.AIAgent.Worker.Models;

namespace Zs.Home.AIAgent.Worker;

public sealed class Worker : BackgroundService
{
    private readonly Kernel _kernel;
    private readonly OpenAiSettings _openAiSettings;

    public Worker(Kernel kernel, IOptions<OpenAiSettings> openAiSettings)
    {
        _kernel = kernel;
        _openAiSettings = openAiSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var promptExecutionSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();

        // Чтобы всё проинициализировалось (TODO: сделать проверку инициализации плагинов)
        await Task.Delay(3000, stoppingToken);

        while (true)
        {
            Console.Write("User > ");
            var userInput = Console.ReadLine();
            Console.WriteLine();
            ConsoleEx.AgentThinking();

            if (_openAiSettings.SaveAndUseHistory)
                chatHistory.AddUserMessage($"{userInput} ");

            var prompt = $"""
                          Ты - русский AI-ассистент, который говорит кратко.
                          Пользователь находится в России, г. Петрозаводск.
                          Ответь на запрос пользователя: {userInput}
                          """;

            var responseBuilder = new StringBuilder();
            var sw = Stopwatch.StartNew();

            // Постепенный вывод ответа
            if (_openAiSettings.StreamingMode)
            {
                var response = _openAiSettings.SaveAndUseHistory
                    ? chatCompletionService.GetStreamingChatMessageContentsAsync(
                        chatHistory: chatHistory,
                        executionSettings: promptExecutionSettings,
                        kernel: _kernel,
                        cancellationToken: stoppingToken)
                    : _kernel.InvokePromptStreamingAsync(prompt, new KernelArguments(promptExecutionSettings), cancellationToken: stoppingToken);

                var isResponding = false;
                await foreach (var chunk in response.WithCancellation(stoppingToken))
                {
                    var responseChunk = chunk.ToString();
                    if (responseChunk.Length > 0)
                    {
                        if (!isResponding)
                        {
                            ConsoleEx.AgentReadyToRespond();
                            isResponding = true;
                        }
                        responseBuilder.Append(responseChunk);
                    }

                    Console.Write(chunk);
                }

                if (_openAiSettings.SaveAndUseHistory)
                    chatHistory.AddAssistantMessage($"{responseBuilder} ");
            }
            // Ожидание и вывод полного ответа за раз
            else
            {
                var response = string.Empty;

                if (_openAiSettings.SaveAndUseHistory)
                {
                    var messageContents = await chatCompletionService.GetChatMessageContentsAsync(chatHistory, promptExecutionSettings, _kernel, stoppingToken);

                    foreach (var chatMessageContent in messageContents)
                        responseBuilder.AppendLine(chatMessageContent.Content);

                    response = responseBuilder.ToString();
                    chatHistory.AddAssistantMessage(responseBuilder.ToString());
                }
                else
                {
                    var functionResult = await _kernel.InvokePromptAsync(prompt, new KernelArguments(promptExecutionSettings), cancellationToken: stoppingToken);
                    responseBuilder.Append(functionResult);
                }

                ConsoleEx.AgentReadyToRespond();
                Console.Write(response);
            }

            Console.WriteLine();
            Console.WriteLine($"[{_openAiSettings.ModelName}, elapsed: {sw.ElapsedMilliseconds} мс]");
            Console.WriteLine();
        }
    }
}
