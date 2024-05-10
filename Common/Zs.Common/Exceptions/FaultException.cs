using System;
using Zs.Common.Extensions;
using Zs.Common.Models;

namespace Zs.Common.Exceptions;

public class FaultException : Exception
{
    public Fault Fault { get; }

    public FaultException(Fault fault, Exception? innerException = null)
        : base(fault.Message, innerException)
    {
        Fault = fault;
    }

    public FaultException()
        : this(Fault.Unknown)
    {
    }

    public FaultException(string message, Exception? innerException = null)
        : this(Fault.Unknown.WithMessage(message), innerException)
    {
    }
}