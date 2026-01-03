using System.Runtime.InteropServices;
using System.Text;

namespace Zs.Common.Utilities;

public static class RuntimeInformationWrapper
{
    public static string GetRuntimeInfo()
    {
        var sb = new StringBuilder()
          .Append("OS: ").Append(RuntimeInformation.OSDescription).Append(' ').Append(RuntimeInformation.OSArchitecture).AppendLine()
          .Append("Framework: ").Append(RuntimeInformation.FrameworkDescription).AppendLine()
          .Append("Process: ").Append(RuntimeInformation.ProcessArchitecture).AppendLine()
          .Append("RuntimeID: ").Append(RuntimeInformation.RuntimeIdentifier);

        return sb.ToString();
    }
}
