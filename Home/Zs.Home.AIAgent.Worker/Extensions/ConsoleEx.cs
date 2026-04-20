namespace Zs.Home.AIAgent.Worker.Extensions;

internal static class ConsoleEx
{
    private const string WaitMessage = "Подождите, готовлю ответ...";
    public static void AgentThinking() => Console.Write($"Agent > {WaitMessage}");

    public static void AgentReadyToRespond()
    {
        Console.Write($"\rAgent > {new string(' ', WaitMessage.Length)}");
        Console.Write("\rAgent > ");
    }
}
