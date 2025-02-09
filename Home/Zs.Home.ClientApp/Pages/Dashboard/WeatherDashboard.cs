using System;
using System.Collections.Generic;
using System.Linq;

namespace Zs.Home.ClientApp.Pages.Dashboard;

public sealed record WeatherDashboard
{
    public required IReadOnlyList<Place> Places { get; init; }
}

public sealed record Place
{
    public required string Name { get; init; }

    public IReadOnlyList<AnalogParameter> Parameters { get; set; } = null!;
}

public sealed record AnalogParameter
{
    public AnalogParameter(string name, Dictionary<DateTime, double> valueLog, string unit)
    {
        Name = name;
        ValueLog = valueLog;
        Unit = unit;
    }

    public AnalogParameter(string name, Dictionary<DateTime, double> valueLog, string unit, WeatherDashboardSettings.Parameter reference)
        : this(name, valueLog, unit)
    {
        Hi = Round(reference.Hi);
        HiHi = Round(reference.HiHi);
        Lo = Round(reference.Lo);
        LoLo = Round(reference.LoLo);
    }

    public string Name { get; }
    public double CurrentValue => ValueLog.OrderByDescending(kvp => kvp.Key).First().Value;
    public double PreviousValue => ValueLog.OrderByDescending(kvp => kvp.Key).ElementAt(1).Value;
    public Dictionary<DateTime, double> ValueLog { get; } = new();
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
            if (LoLo < CurrentValue && CurrentValue < Lo || Hi < CurrentValue && CurrentValue < HiHi)
                return Status.Warning;
            if (LoLo > CurrentValue || CurrentValue > HiHi)
                return Status.Danger;

            return Status.Ok;
        }
    }

    public Forecast Forecast
    {
        get
        {
            if (Status is Status.Ok)
                return Forecast.Normal;

            if (Status is Status.Warning)
            {
                if (CurrentValue < Lo && CurrentValue < PreviousValue || CurrentValue > Hi && CurrentValue > PreviousValue)
                    return Forecast.Warning;

                if (CurrentValue < Lo && CurrentValue > PreviousValue || CurrentValue > Hi && CurrentValue < PreviousValue)
                    return Forecast.Good;
            }

            if (Status is Status.Danger)
            {
                if (CurrentValue < LoLo && CurrentValue < PreviousValue || CurrentValue > HiHi && CurrentValue > PreviousValue)
                    return Forecast.Danger;

                if (CurrentValue < LoLo && CurrentValue > PreviousValue || CurrentValue > HiHi && CurrentValue < PreviousValue)
                    return Forecast.Good;
            }

            throw new NotImplementedException();
        }
    }

    private static double? Round(double? rawValue)
        => rawValue.HasValue ? Math.Round(rawValue.Value, 2, MidpointRounding.AwayFromZero) : default;

    public override string ToString() => $"{CurrentValue} {Unit}";
}

public enum Status { Ok, Warning, Danger }

public enum Forecast { Normal, Good, Warning, Danger }
