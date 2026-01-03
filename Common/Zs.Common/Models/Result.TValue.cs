namespace Zs.Common.Models;

public class Result<TValue> : Result
{
    private readonly TValue _value;

    public TValue Value
    {
        get
        {
            EnsureSuccess();
            return _value;
        }
    }

    public Result(TValue value, Fault? fault = null)
        : base(fault)
    {
        _value = value;
    }

    public static implicit operator Result<TValue>(TValue value) => new(value);

    public static implicit operator Result<TValue>(Fault fault) => new(default!, fault);
}