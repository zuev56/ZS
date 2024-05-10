using System;
using System.IO;

namespace Zs.Common.Extensions;

public static class ExceptionExtensions
{
    public static string ToText(this Exception ex)
        => $"\n\n{ex.GetType()}\nMessage:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}";

    public static bool TrySaveToFile(this Exception exception)
    {
        try
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), $"Critical_failure_{DateTime.Now:yyyy.MM.dd HH:mm:ss.ff}.log");
            var text = $"The exception {exception.GetType()} has occured{Environment.NewLine}"
                       + $"Message: {exception.Message}{Environment.NewLine}"
                       + $"StackTrace: {exception.StackTrace}";
            File.AppendAllText(path, text);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n{ex}\nMessage:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}");
            return false;
        }
    }
}