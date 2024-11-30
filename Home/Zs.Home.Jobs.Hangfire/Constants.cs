namespace Zs.Home.Jobs.Hangfire;

internal static class Constants
{
    internal const string HomeDbConnectionString = "Default";
    internal const string HangfireSchema = "hangfire";
    internal const string WeatherRegistratorSchema = "weather";
    internal const string PostgresUtcNowValue = "now() at time zone ('utc')";
}
