using System;
using System.Collections.Generic;
using System.Linq;

namespace Zs.Home.ClientApp.Pages.Dashboard.Weather;

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
    public Dictionary<DateTime, double> ValueLog { get; } = new();
    public string Unit { get; }
    public double? Hi { get; init; }
    public double? HiHi { get; init; }
    public double? Lo { get; init; }
    public double? LoLo { get; init; }
    public short Order { get; set; } = short.MaxValue;

    public double CurrentValue => ValueLog.OrderByDescending(kvp => kvp.Key).First().Value;
    public double PreviousValue => ValueLog.OrderByDescending(kvp => kvp.Key).ElementAt(1).Value;

    public Dynamic Dynamic => (CurrentValue - PreviousValue) switch
    {
        var delta when Math.Abs(delta) < 0.01 => Dynamic.Stable,
        < 0 => Dynamic.Negative,
        > 0 => Dynamic.Positive,
    };

    public Status Status
    {
        get
        {
            if (LoLo < CurrentValue && CurrentValue < Lo) return Status.WarningLo;
            if (Hi < CurrentValue && CurrentValue < HiHi) return Status.WarningHi;
            if (LoLo > CurrentValue) return Status.DangerLoLo;
            if (CurrentValue > HiHi) return Status.DangerHiHi;

            return Status.Normal;
        }
    }

    public Forecast Forecast
    {
        get
        {
            if (Status is Status.Normal || Dynamic == Dynamic.Stable)
                return Forecast.Normal;

            if (Dynamic == Dynamic.Positive)
            {
                switch (Status)
                {
                    case Status.WarningLo: return Forecast.Good;
                    case Status.WarningHi: return Forecast.Warning;
                    case Status.DangerLoLo: return Forecast.Good;
                    case Status.DangerHiHi: return Forecast.Danger;
                }
            }

            if (Dynamic == Dynamic.Negative)
            {
                switch (Status)
                {
                    case Status.WarningLo: return Forecast.Warning;
                    case Status.WarningHi: return Forecast.Good;
                    case Status.DangerLoLo: return Forecast.Danger;
                    case Status.DangerHiHi: return Forecast.Good;
                }
            }

            throw new NotImplementedException();
        }
    }

    private static double? Round(double? rawValue)
        => rawValue.HasValue ? Math.Round(rawValue.Value, 2, MidpointRounding.AwayFromZero) : default;

    public override string ToString() => $"{CurrentValue} {Unit}";
}

public enum Status { DangerLoLo, WarningLo, Normal, WarningHi, DangerHiHi }

public enum Dynamic { Stable, Positive, Negative }

public enum Forecast { Normal, Good, Warning, Danger }
