namespace Zs.Home.AIAgent.Worker.Extensions;

internal static class ConsoleEx
{
    private const string WaitMessage = "Подождите, готовлю ответ...";
    public static void AgentThinking() => Console.Write($"Agent > {WaitMessage}");

    public static void AgentReadyToResponse()
    {
        Console.CursorLeft = 0;
        Console.Write(new string(' ', Console.WindowWidth));
        Console.CursorLeft = 0;
        Console.Write("\rAgent > ");
    }
}
