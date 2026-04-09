using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Zs.Common.Utilities;

public static class RuntimeInformationWrapper
{
    public static string GetRuntimeInfo()
        => $"""
            Host: {(Environment.GetEnvironmentVariable("HOSTNAME") ?? "unknown")}
            OS: {RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture}
            Framework: {RuntimeInformation.FrameworkDescription}
            Process: {RuntimeInformation.ProcessArchitecture}
            RuntimeID: {RuntimeInformation.RuntimeIdentifier}
            Time: {DateTime.Now:s} ({TimeZoneInfo.Local})
            Culture: {CultureInfo.CurrentCulture.Name}
            """;
}
