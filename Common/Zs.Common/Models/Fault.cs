using System;
using System.Collections.Generic;
#nullable enable
namespace Zs.Common.Models;

public sealed class Fault
{
    public string Code { get; }
    public string Message { get; }
    public IReadOnlyCollection<Fault>? Details { get; }
    public IReadOnlyDictionary<string, object>? Context { get; set; }
    public static Fault Unknown { get; } = new(nameof(Unknown));

    public Fault(string code, string? message = null, IEnumerable<Fault>? details = null)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? $"Fault '{code}' has occured";
        Details = details is null ? null : new List<Fault>(details);
    }

    public Fault(string code, string? message, Fault details)
    {
        Code = code;
        Message = message ?? $"Fault '{code}' has occured";
        Details = new List<Fault>{details};
    }

    public static implicit operator Fault(string message)
    {
        return new Fault(nameof(Unknown), message);
    }

    public override string ToString() => $"Code: {Code}, Message: {Message}";
}
