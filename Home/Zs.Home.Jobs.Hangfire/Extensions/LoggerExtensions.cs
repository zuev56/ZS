using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;

namespace Zs.Home.Jobs.Hangfire.Extensions;

internal static class LoggerExtensions
{
    internal static void LogJobStart(this ILogger logger, [CallerFilePath] string? jobName = null)
    {
        if (Path.Exists(jobName))
            jobName = Path.GetFileNameWithoutExtension(jobName);

        logger.LogDebugIfNeed("Start {Job}", jobName);
    }

    internal static void LogJobFinish(this ILogger logger, TimeSpan elapsed, [CallerFilePath] string? jobName = null)
    {
        if (Path.Exists(jobName))
            jobName = Path.GetFileNameWithoutExtension(jobName);

        logger.LogDebugIfNeed("Finish {Job}, elapsed: {Elapsed}", jobName, elapsed);
    }
}
