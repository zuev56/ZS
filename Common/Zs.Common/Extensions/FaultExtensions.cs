using System;
using Zs.Common.Models;

namespace Zs.Common.Extensions;

public static class FaultExtensions
{
    public static Fault WithMessage(this Fault fault, string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return new Fault(fault.Code, message, fault.Details);
    }
}