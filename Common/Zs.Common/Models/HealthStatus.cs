using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Zs.Common.Models;

public sealed record HealthStatus
{
    private static readonly string[] _sizeName = [ "Bytes", "KB", "MB", "GB", "TB" ];
    public TimeSpan ProcessRunningTime { get; private init; }
    public CpuTime CpuTime { get; private init; } = null!;
    public MemoryUsage MemoryUsage { get; private init; } = null!;
    public int ActiveThreads { get; private init; }
    public IReadOnlyList<DbTableInfo>? DbTableInfos { get; private init; }

    private HealthStatus()
    {
    }

    public static HealthStatus Get(Process process, DbTableInfo[] dbTables)
    {
        return new HealthStatus
        {
            ProcessRunningTime = DateTime.Now - process.StartTime,
            CpuTime = new CpuTime
            (
                process.TotalProcessorTime,
                process.UserProcessorTime,
                process.PrivilegedProcessorTime
            ),
            MemoryUsage = new MemoryUsage
            (
                BytesToHumanReadableSize(process.WorkingSet64),
                BytesToHumanReadableSize(process.PeakWorkingSet64)
            ),
            ActiveThreads = process.Threads.Count,
            DbTableInfos = dbTables
        };
    }

    private static string BytesToHumanReadableSize(long bytes)
    {
        var num = (int)Math.Floor(Math.Log(bytes) / Math.Log(1024.0));
        return Math.Round(bytes / Math.Pow(1024.0, num), 2) + " " + _sizeName[num];
    }
}

public sealed record CpuTime(TimeSpan Total, TimeSpan User, TimeSpan Privileged);
public sealed record MemoryUsage(string Current, string Peak);
public sealed record DbTableInfo(string Table, long Rows, string Size);
