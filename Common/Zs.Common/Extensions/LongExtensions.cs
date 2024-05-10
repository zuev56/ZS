using System;

namespace Zs.Common.Extensions;

public static class LongExtensions
{
    public static string BytesAmountToSizeString(this long bytesAmount)
    {
        var units = new[] { "Bytes", "KB", "MB", "GB", "TB" };

        if (bytesAmount == 1L)
            return "1 Byte";

        var num = (int)Math.Floor(Math.Log(bytesAmount) / Math.Log(1024.0));
        return Math.Round(bytesAmount / Math.Pow(1024.0, num), 2) + " " + units[num];
    }
}