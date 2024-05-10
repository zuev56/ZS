namespace Zs.Parser.EspMeteo.Models;

public sealed class Parameter
{
    public string Name { get; }
    public float? Value { get; }
    public string Unit { get; }

    public Parameter(string name, string unit)
    {
        Name = name;
        Unit = unit;
    }

    public Parameter(string name, float value, string unit)
        : this(name, unit)
    {
        Value = value;
    }
}