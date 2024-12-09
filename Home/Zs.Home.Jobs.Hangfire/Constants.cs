namespace Zs.Home.Jobs.Hangfire;

internal static class Constants
{
    internal const string HomeDbConnectionString = "Default";
    internal const string HangfireSchema = "hangfire";

    internal const string Temperature = nameof(Temperature);
    internal const string Humidity = nameof(Humidity);
    internal const string Pressure = nameof(Pressure);
    internal const string Co2 = nameof(Co2);
}
