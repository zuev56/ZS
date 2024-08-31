using ChatAdmin.Bot.Abstractions;
using ChatAdmin.Bot.Data;
using ChatAdmin.Bot.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Telegram.Bot;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.PostgreSQL;
using Zs.Bot.Data.PostgreSQL.Repositories;
using Zs.Bot.Data.Repositories;
using Zs.Bot.Messenger.Telegram;
using Zs.Bot.Services.Commands;
using Zs.Bot.Services.DataSavers;
using Zs.Bot.Services.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Connection;
using Zs.Common.Services.Logging.Seq;
using Zs.Common.Services.Scheduler;
using Zs.Common.Services.Shell;

namespace ChatAdmin.Bot;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(CreateConfiguration(args), "Serilog")
                .CreateLogger();

            Log.Warning("-! Starting {ProcessName} (MachineName: {MachineName}, OS: {OS}, User: {User}, ProcessId: {ProcessId})",
                Process.GetCurrentProcess().MainModule.ModuleName,
                Environment.MachineName,
                Environment.OSVersion,
                Environment.UserName,
                Environment.ProcessId);

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            await CreateHostBuilder(args).RunConsoleAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ProgramUtilites.TrySaveFailInfo(ex.ToText());
            Console.WriteLine(ex.ToText());
            Console.Read();
        }
    }

    private static IConfiguration CreateConfiguration(string[] args)
    {
        if (!File.Exists(ProgramUtilites.MainConfigurationPath))
            throw new AppsettingsNotFoundException();

        var configuration = new ConfigurationManager();
        configuration.AddJsonFile(ProgramUtilites.MainConfigurationPath, optional: false, reloadOnChange: true);

        foreach (var arg in args)
        {
            if (!File.Exists(arg))
                throw new FileNotFoundException($"Wrong configuration path:\n{arg}");

            configuration.AddJsonFile(arg, optional: true, reloadOnChange: true);
        }

        AssertConfigurationIsCorrect(configuration);

        return configuration;
    }

    private static void AssertConfigurationIsCorrect(IConfiguration configuration)
    {
        // TODO
        // BotToken
        // ConnectionStrings
        // "ChatAdmin": { "Chat": { "Id": 2, "TimeZone": "MSK" },
        // UnaccountedUserIds
        // "BotUserId": -1,
        // "MessageLimitHi": 3,
        // "MessageLimitHiHi": 5,
        // "MessageLimitAfterBan": 2,
        // "AccountingStartsAfter": 2,
        // "WaitAfterConnectionRepairedSec": 30
        //}
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostContext, configurationBuilder) => configurationBuilder.AddConfiguration(CreateConfiguration(args)))
            .UseSerilog()
            .ConfigureServices((hostContext, services) =>
            {
                var connectionString = hostContext.Configuration.GetSecretValue("ConnectionStrings:Default");

                services.AddDbContextFactory<ChatAdminContext>(optionsBuilder => { optionsBuilder.UseNpgsql(connectionString); });
                services.AddDbContextFactory<PostgreSqlBotContext>(optionsBuilder => { optionsBuilder.UseNpgsql(connectionString); });

                services.AddSingleton<IConnectionAnalyser, ConnectionAnalyser>(sp =>
                {
                    var connectionAnalyzer = new ConnectionAnalyser(
                        sp.GetService<ILogger<ConnectionAnalyser>>(),
                        hostContext.Configuration.GetSection("ConnectionAnalyser:Urls").Get<string[]>());

                    if (hostContext.Configuration.GetValue<bool>("Proxy:UseProxy"))
                    {
                        connectionAnalyzer.InitializeProxy(hostContext.Configuration["Proxy:Socket"],
                            hostContext.Configuration.GetSecretValue("Proxy:Login"),
                            hostContext.Configuration.GetSecretValue("Proxy:Password"));

                        HttpClient.DefaultProxy = connectionAnalyzer.WebProxy;
                    }
                    return connectionAnalyzer;
                });

                services.AddSingleton<ISeqService, SeqService>(_ =>
                    new SeqService(hostContext.Configuration["Seq:ServerUrl"], hostContext.Configuration.GetSecretValue("Seq:ApiToken")));

                services.AddSingleton<ITelegramBotClient>(_ =>
                    new TelegramBotClient(hostContext.Configuration.GetSecretValue("Bot:Token"), new HttpClient()));

                services.AddSingleton<IMessenger, TelegramMessenger>();

                services.AddSingleton<HttpClient>();
                services.AddSingleton<IMessageDataSaver, MessageDataDBSaver>();
                services.AddSingleton<ChatStateService>();
                services.AddSingleton<IMessageProcessor, MessageProcessor>();
                services.AddSingleton<IScheduler, Scheduler>();
                services.AddSingleton<IShellLauncher, ShellLauncher>(sp =>
                    new ShellLauncher(
                        bashPath: hostContext.Configuration.GetSecretValue("Bot:BashPath"),
                        powerShellPath: hostContext.Configuration.GetSecretValue("Bot:PowerShellPath")
                    ));
                services.AddSingleton<ICommandManager, CommandManager>();

                services.AddSingleton<IBansRepository, BansRepository<ChatAdminContext>>();
                services.AddSingleton<ICommandsRepository, CommandsRepository<PostgreSqlBotContext>>();
                services.AddSingleton<IUserRolesRepository, UserRolesRepository<PostgreSqlBotContext>>();
                services.AddSingleton<IChatsRepository, ChatsRepository<PostgreSqlBotContext>>();
                services.AddSingleton<IUsersRepository, UsersRepository<PostgreSqlBotContext>>();
                services.AddSingleton<IMessagesRepository, MessagesRepository<PostgreSqlBotContext>>();
                services.AddSingleton<IDbClient, DbClient>(sp =>
                    new DbClient(
                        connectionString,
                        sp.GetService<ILogger<DbClient>>())
                    );

                using (var serviceScope = services.BuildServiceProvider().GetService<IServiceScopeFactory>().CreateScope())
                {
                    var chatAdminContext = serviceScope.ServiceProvider.GetRequiredService<ChatAdminContext>();
                    chatAdminContext.Database.Migrate();
                    var botContext = serviceScope.ServiceProvider.GetRequiredService<PostgreSqlBotContext>();
                    botContext.Database.Migrate();
                }

                services.AddHostedService<ChatAdmin>();
            });
    }

}
