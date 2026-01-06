// Import packages

using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// const string _llm = "google/gemma-3-1b";
// const string _uri = "http://localhost:1234/v1";
const string _llm = "qwen/qwen3-1.7b";
const string _uri = "http://192.168.1.104:1234/v1";

#pragma warning disable SKEXP0010
IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddOpenAIChatCompletion(
    modelId: _llm,
    apiKey: null,
    endpoint: new Uri(_uri)
);
Kernel kernel = kernelBuilder.Build();

#pragma warning disable SKEXP0010
OpenAIChatCompletionService chatCompletionService = new (
    modelId: _llm,
    apiKey: null,
    endpoint: new Uri(_uri)
);


var history = new ChatHistory();
// history.AddUserMessage("Разговаривай по русски.");

// history.AddUserMessage("Ты — ИИ-агент, который выполняет действия. Никогда не рассуждай вслух и не пиши размышления. Просто выполни действие в формате: {\"action\": \"...\", \"args\": {...}}.");

// var response = await chatCompletionService.GetChatMessageContentAsync(
//     history,
//     kernel: kernel
// );
//
//
// // Add a plugin (the LightsPlugin class is defined below)
kernel.Plugins.AddFromType<LightsPlugin>("Lights");

// Enable planning
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};


// Initiate a back-and-forth chat
string? userInput;
do {
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    // Add user input
    history.AddUserMessage($"{userInput} /no_think");

    var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
        chatHistory: history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel
    );

    var sw = Stopwatch.StartNew();

    await foreach (var chunk in response)
    {
        if (chunk.Content?.Contains("<think>") == true || chunk.Content?.Contains("</think>") == true || chunk.Content == "\n\n")
            continue;

        Console.Write(chunk);
    }
    // var result = response.
    // // Get the response from the AI
    // var result = await chatCompletionService.GetChatMessageContentAsync(
    //     history,
    //     // executionSettings: openAIPromptExecutionSettings,
    //     kernel: kernel);
    //
    // // Print the results
    // Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    // history.AddMessage(result.Role, result.Content ?? string.Empty);

    Console.Write($" ({sw.ElapsedMilliseconds} мс)");
    Console.WriteLine();
} while (userInput is not null);

/// <summary>
/// Подключаемый модуль, который может управлять лампочкой
/// </summary>
public class LightsPlugin
{
    private readonly List<LightModel> _lights =
    [
        new() {Id = 1, Name = "Настольная лампа", IsOn = false},
        new() {Id = 2, Name = "Гостинная", IsOn = false},
        new() {Id = 3, Name = "Кухня", IsOn = true}
    ];

    [KernelFunction("get_lights")]
    [Description("Получить список светильников и их текущее состояние")]
    [return: Description("Список светильников")]
    public async Task<List<LightModel>> GetLightsAsync()
    {
        return _lights;
    }

    [KernelFunction("change_state")]
    [Description("Изменить состояние светильника")]
    [return: Description("Обновлённое состояние светильника; Вернёт null если светильник не существует")]
    public async Task<LightModel?> ChangeStateAsync(int id, bool isOn)
    {
        var light = _lights.FirstOrDefault(light => light.Id == id);

        if (light == null)
            return null;

        // Update the light with the new state
        light.IsOn = isOn;

        return light;
    }
}

public sealed class LightModel
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("is_on")]
    public bool? IsOn { get; set; }
}
