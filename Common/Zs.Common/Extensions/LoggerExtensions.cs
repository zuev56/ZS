using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Zs.Common.Extensions;

public static class LoggerExtensions
{
    public static void LogProgramStart(this ILogger logger)
    {
        logger.LogWarning("-! Starting {ProcessName} (MachineName: {MachineName}, OS: {OS}, User: {User}, ProcessId: {ProcessId})",
            Process.GetCurrentProcess().MainModule?.ModuleName,
            Environment.MachineName,
            Environment.OSVersion,
            Environment.UserName,
            Environment.ProcessId);
    }

    public static void LogProcessState(this ILogger logger, Process process)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(process);

        var logStr = "Process running time: {RunningTime:G}, CPU time: [ Total: {CPU_TotalTime}, User: {CPU_UserTime}, Privileged: {CPU_PrivilegedTime} ], "
                      + "Memory usage: [ Current: {RAM_Current}, Peak: {RAM_Peak} ], "
                      + "Active threads: {ThreadsCount}";

        logger.LogInformation(logStr, (DateTime.Now - process.StartTime),
            process.TotalProcessorTime, process.UserProcessorTime, process.PrivilegedProcessorTime,
            process.WorkingSet64.BytesAmountToSizeString(), process.PeakWorkingSet64.BytesAmountToSizeString(),
            process.Threads.Count);
    }

    #region Perfomance improvements (https://andrewlock.net/exploring-dotnet-6-part-8-improving-logging-performance-with-source-generators/)
    public static void LogTraceIfNeed(this ILogger logger, string? message, params object?[] args)
    {
        if (logger.IsEnabled(LogLevel.Trace))
            logger.LogTrace(message, args);
    }

    public static void LogDebugIfNeed(this ILogger logger, string? message, params object?[] args)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug(message, args);
    }

    public static void LogInformationIfNeed(this ILogger logger, string? message, params object?[] args)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(message, args);
    }

    public static void LogWarningIfNeed(this ILogger logger, string? message, params object?[] args)
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning(message, args);
    }

    public static void LogErrorIfNeed(this ILogger logger, string? message, params object?[] args)
    {
        if (logger.IsEnabled(LogLevel.Error))
            logger.LogError(message, args);
    }

    public static void LogErrorIfNeed(this ILogger logger, Exception? exception, string? message, params object?[] args)
    {
        if (!logger.IsEnabled(LogLevel.Error))
            return;

        if (exception?.Data.Count > 0)
            args = args.Append(exception.Data).ToArray();

        logger.LogError(exception, message, args);
    }

    public static void LogCriticalIfNeed(this ILogger logger, string? message, params object?[] args)
    {
        if (logger.IsEnabled(LogLevel.Critical))
            logger.LogCritical(message, args);
    }
    #endregion
}