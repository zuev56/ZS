using System;
using System.Collections.Generic;
using System.Linq;
using Zs.Home.WebApi;

namespace Zs.Home.ClientApp.Pages.Dashboard.Weather;

public record struct LogValue(double Value, Status Status, bool IsBasic = true);

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
    public AnalogParameter(string name, Dictionary<DateTime, LogValue> valueLog, string unit)
    {
        Name = name;
        ValueLog = valueLog;
        Unit = unit;
    }

    public AnalogParameter(string name, Dictionary<DateTime, LogValue> valueLog, string unit, ParameterSettings settings)
        : this(name, valueLog, unit)
    {
        Hi = Round(settings.Hi);
        HiHi = Round(settings.HiHi);
        Lo = Round(settings.Lo);
        LoLo = Round(settings.LoLo);
    }

    public string Name { get; }
    public Dictionary<DateTime, LogValue> ValueLog { get; } = new();

    public string Unit { get; }
    public double? Hi { get; init; }
    public double? HiHi { get; init; }
    public double? Lo { get; init; }
    public double? LoLo { get; init; }
    public short Order { get; set; } = short.MaxValue;

    public double CurrentValue => ValueLog.OrderByDescending(kvp => kvp.Key).First().Value.Value;
    public double PreviousValue => ValueLog.OrderByDescending(kvp => kvp.Key).ElementAt(1).Value.Value;

    public Dynamic Dynamic => (CurrentValue - PreviousValue) switch
    {
        var delta when Math.Abs(delta) < 0.01 => Dynamic.Stable,
        < 0 => Dynamic.Negative,
        > 0 => Dynamic.Positive,
    };

    public Status Status => CalculateStatus(CurrentValue);

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
        => rawValue.HasValue ? Math.Round(rawValue.Value, 2, MidpointRounding.AwayFromZero) : 0.0;

    public Status CalculateStatus(double value)
    {
        if (LoLo < value && value < Lo) return Status.WarningLo;
        if (Hi < value && value < HiHi) return Status.WarningHi;
        if (LoLo > value) return Status.DangerLoLo;
        if (value > HiHi) return Status.DangerHiHi;

        return Status.Normal;
    }

    public override string ToString() => $"{CurrentValue} {Unit}";
}

public enum Status { DangerLoLo, WarningLo, Normal, WarningHi, DangerHiHi }

public enum Dynamic { Stable, Positive, Negative }

public enum Forecast { Normal, Good, Warning, Danger }
