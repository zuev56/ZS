using Zs.Common.Models;

namespace Zs.DemoBot;

internal static class Faults
{
    public static Fault Unauthorized => new (nameof(Unauthorized));
}