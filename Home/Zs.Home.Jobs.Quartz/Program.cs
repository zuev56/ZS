using Microsoft.Extensions.Hosting;
using Quartz;
using Zs.Home.Jobs;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddQuartz(q =>
{
    q.AddJobAndTrigger<HelloWorldJob>(builder.Configuration);
});

builder.Services.AddQuartzHostedService(
    static q => q.WaitForJobsToComplete = true);

await builder.Build().RunAsync();
