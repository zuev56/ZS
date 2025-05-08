using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Connection;
using Zs.Common.Services.Logging.DelayedLogger;
using Zs.Common.Services.Scheduling;
using Zs.VkActivity.Common;
using Zs.VkActivity.Common.Abstractions;
using Zs.VkActivity.Data.Abstractions;
using Zs.VkActivity.Worker.Abstractions;
using static Zs.VkActivity.Worker.Models.Constants;

namespace Zs.VkActivity.Worker.Services;

internal sealed class WorkerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IScheduler _scheduler;
    private readonly IConnectionAnalyzer _connectionAnalyzer;
    private readonly IConfiguration _configuration;
    private readonly IDelayedLogger<WorkerService> _delayedLogger;
    private readonly ILogger<WorkerService> _logger;
    private DateTime _disconnectionTime = DateTime.UtcNow;

    private Job _userActivityLoggerJob = null!;
    private bool _isFirstStep = true;


    public WorkerService(
        IServiceScopeFactory serviceScopeFactory,
        IScheduler scheduler,
        IConnectionAnalyzer connectionAnalyzer,
        IConfiguration configuration,
        IDelayedLogger<WorkerService> delayedLogger,
        ILogger<WorkerService> logger)
    {
        _scopeFactory = serviceScopeFactory;
        _scheduler = scheduler;
        _connectionAnalyzer = connectionAnalyzer;
        _configuration = configuration;
        _delayedLogger = delayedLogger;
        _logger = logger;

        _scheduler.Jobs.AddRange(CreateJobs());
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connectionAnalyzer.ConnectionStatusChanged += ConnectionAnalyzer_StatusChanged;
        _connectionAnalyzer.Start(2.Seconds(), 3.Seconds());

        _scheduler.Start(3.Seconds(), 1.Seconds());

        _logger.LogInformation($"{nameof(WorkerService)} started.");
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _connectionAnalyzer.ConnectionStatusChanged -= ConnectionAnalyzer_StatusChanged;
        _connectionAnalyzer.Stop();

        _scheduler.Stop();

        _logger.LogInformation($"{nameof(WorkerService)} stopped.");
        return Task.CompletedTask;
    }

    private void ConnectionAnalyzer_StatusChanged(ConnectionStatus status)
    {
        _disconnectionTime = status == ConnectionStatus.Ok
            ? default
            : DateTime.UtcNow;
    }

    private List<JobBase> CreateJobs()
    {
        _userActivityLoggerJob = new ProgramJob(
            period: TimeSpan.FromSeconds(_configuration.GetSection(AppSettings.Vk.ActivityLogIntervalSec).Get<int>()),
            method: SaveVkUsersActivityAsync,
            description: nameof(_userActivityLoggerJob),
            logger: _logger);

        var userDataUpdaterJob = new ProgramJob(
            period: TimeSpan.FromHours(_configuration.GetSection(AppSettings.Vk.UsersDataUpdateIntervalHours).Get<int>()),
            startUtcDate: DateTime.UtcNow.Date.AddDays(1),
            method: UpdateUsersDataAsync,
            description: "userDataUpdaterJob",
            logger: _logger);

        return
        [
            _userActivityLoggerJob,
            userDataUpdaterJob
        ];
    }

    /// <summary>Activity data collection</summary>
    private async Task SaveVkUsersActivityAsync()
    {
        var sw = Stopwatch.StartNew();
        _logger.LogTraceIfNeed("Start SaveVkUsersActivityAsync");

        if (_disconnectionTime != default)
        {
            await SetUndefinedActivityToAllUsers();

            _delayedLogger.LogError(NoInternetConnection);
            return;
        }

        if (_isFirstStep)
        {
            _isFirstStep = false;
            _userActivityLoggerJob.IdleStepsCount = 1;
            await AddUsersFullInfoAsync();
            return;
        }

        if (_userActivityLoggerJob.IdleStepsCount > 0)
        {
            _userActivityLoggerJob.IdleStepsCount = 0;
        }

        var result = await SaveUsersActivityAsync();
        if (!result.Successful)
        {
            _logger.LogErrorIfNeed(result.Fault!.Code);
        }

        _logger.LogTraceIfNeed("Finish SaveVkUsersActivityAsync. Elapsed: {Elapsed}", sw.Elapsed);
    }

    private async Task<Result> SaveUsersActivityAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var activityLoggerService = scope.ServiceProvider.GetRequiredService<IActivityLogger>();
        return await activityLoggerService.SaveUsersActivityAsync();
    }

    private async Task SetUndefinedActivityToAllUsers()
    {
        _logger.LogTraceIfNeed("Set undefined activity to all users");

        using var scope = _scopeFactory.CreateScope();
        var activityLoggerService = scope.ServiceProvider.GetRequiredService<IActivityLogger>();
        var setUndefinedActivityResult = await activityLoggerService.ChangeAllUserActivitiesToUndefinedAsync();

        if (!setUndefinedActivityResult.Successful)
            _logger.LogError(string.Join(Environment.NewLine, setUndefinedActivityResult.Fault!.Code));
    }
    private async Task AddUsersFullInfoAsync()
    {
        _logger.LogTraceIfNeed("Add users full infos");

        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        var initialUserIds = _configuration.GetSection(AppSettings.Vk.InitialUserIds).Get<string[]>();
        var addResult = await userManager.AddUsersAsync(initialUserIds ?? []);

        if (!addResult.Successful)
            _logger.LogError(string.Join(Environment.NewLine, addResult.Fault!.Code), new { UserIds = initialUserIds });
    }

    private async Task UpdateUsersDataAsync()
    {
        _logger.LogTraceIfNeed("Start UpdateUsersDataAsync");

        using var scope = _scopeFactory.CreateScope();
        var usersRepo = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var userIds = await usersRepo.FindAllIdsAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        var updateResult = await userManager.UpdateUsersAsync(userIds);

        if (!updateResult.Successful)
            _logger.LogError(string.Join(Environment.NewLine, updateResult.Fault!.Code), new { UserIds = userIds });

        _logger.LogTraceIfNeed("Finish UpdateUsersDataAsync");
    }
}
