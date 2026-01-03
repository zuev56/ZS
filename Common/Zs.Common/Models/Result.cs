using System;
using Zs.Common.Exceptions;

namespace Zs.Common.Models;

public class Result
{
    public Fault? Fault { get; }
    public bool Successful => Fault == null;

    protected Result(Fault? fault)
    {
        Fault = fault;
    }

    public static Result Success() => new(null);

    public static Result<TValue> Success<TValue>(TValue value) => new(value);

    public static implicit operator Result(Fault fault) => Fail(fault);

    public static Result Fail(Fault fault)
    {
        ArgumentNullException.ThrowIfNull(fault);
        return new Result(fault);
    }

    public static Result<TValue> Fail<TValue>(Fault fault)
    {
        ArgumentNullException.ThrowIfNull(fault);
        return new Result<TValue>(default!, fault);
    }

    public virtual void EnsureSuccess()
    {
        if (!Successful)
        {
            throw new FaultException(Fault!);
        }
    }
}
