namespace Zs.Home.Jobs.Hangfire.Hangfire;

public interface ICronSettings
{
    public string CronExpression { get; }
}
