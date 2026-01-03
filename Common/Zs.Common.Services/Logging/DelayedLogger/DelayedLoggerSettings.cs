namespace Zs.Common.Services.Logging.DelayedLogger;

public sealed class DelayedLoggerSettings
{
    public const string SectionName = "DelayedLogger";

    /// <summary>
    /// The interval for writing messages with the specified content to the log.
    /// </summary>
    public int DefaultLogWriteIntervalMs { get; init; } = 5 * 60 * 1000;

    /// <summary>
    /// During processing, the accumulated messages are analyzed
    /// and a decision is made on the need to add an entry to the log.
    /// </summary>
    public int ProcessIntervalMs { get; init; } = 5000;
}
