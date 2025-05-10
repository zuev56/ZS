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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
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
using Zs.Common.Extensions;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Connection;
using Zs.Common.Services.Logging.Seq;
using Zs.Common.Services.Scheduler;
using Zs.Common.Services.Shell;
using Zs.Common.TempExtensions;
using Path = System.IO.Path;

namespace ChatAdmin.Bot;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args)
            .ConfigureExternalAppConfiguration(args, Assembly.GetAssembly(typeof(Program))!)
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogProgramStartup();

        await host.RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(configurationBuilder =>
            {
                if (args?.Any() != true)
                    return;

                foreach (var arg in args.Distinct().Where(a => !string.IsNullOrWhiteSpace(a)))
                {
                    if (File.Exists(arg) && Path.GetExtension(arg).Equals(".json", StringComparison.InvariantCultureIgnoreCase))
                        configurationBuilder.AddJsonFile(arg);

                    var configFilePath = Path.Combine(arg, $"{Assembly.GetAssembly(typeof(Program))!.GetName().Name}.json");
                    if (File.Exists(configFilePath))
                        configurationBuilder.AddJsonFile(configFilePath);
                }

                var configFiles = configurationBuilder.Sources
                    .Where(s => s is FileConfigurationSource)
                    .Select(s => ((FileConfigurationSource)s).Path)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList()!;
                Console.WriteLine($"Applied configuration files: {string.Join(", ", configFiles)}");
            })
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
