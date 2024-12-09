using System;
using System.Collections.Generic;

namespace Zs.Home.ClientApp.Pages.Dashboard;

public sealed record WeatherDashboard
{
    public required IReadOnlyList<Place> Places { get; set; }
}

public sealed record Place
{
    public required string Name { get; init; }

    public IReadOnlyList<AnalogParameter> Parameters { get; set; }
}

public sealed record AnalogParameter
{
    public AnalogParameter(string name, double value, string unit)
    {
        Name = name;
        Value = Round(value)!.Value;
        Unit = unit;
    }

    public AnalogParameter(string name, double value, string unit, WeatherDashboardSettings.Parameter reference)
        : this(name, value, unit)
    {
        Hi = Round(reference.Hi);
        HiHi = Round(reference.HiHi);
        Lo = Round(reference.Lo);
        LoLo = Round(reference.LoLo);
    }

    public string Name { get; }
    public double Value { get; }
    public string Unit { get; }
    public double? Hi { get; init; }
    public double? HiHi { get; init; }
    public double? Lo { get; init; }
    public double? LoLo { get; init; }

    public short Order { get; set; } = short.MaxValue;

    public Status Status
    {
        get
        {
            if (LoLo < Value && Value < Lo || Hi < Value && Value < HiHi)
                return Status.Warning;
            if (LoLo > Value || Value > HiHi)
                return Status.Danger;

            return Status.Ok;
        }
    }

    private static double? Round(double? rawValue)
        => rawValue.HasValue ? Math.Round(rawValue.Value, 2, MidpointRounding.AwayFromZero) : default;

    public override string ToString() => $"{Value} {Unit}";
}

public enum Status { Ok, Warning, Danger }
