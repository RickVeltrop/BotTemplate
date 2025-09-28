using BotTemplate.Services;
using DSharpPlus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;

namespace BotTemplate;

public static class Program
{
    private static readonly string? token = Environment.GetEnvironmentVariable("TOKEN");
    public static readonly IServiceProvider serviceProvider = CreateProvider();
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private static LoggingConfiguration LoggerConfig()
    {
        var config = serviceProvider.GetRequiredService<IConfiguration>();

        var logConsole = new ConsoleTarget("logconsole") { Layout = config.GetSection("ConsoleLoggerLayout").Value };
        var logFile = new FileTarget("logfile") { FileName = "${basedir}/logs/${shortdate}.log", Layout = config.GetSection("FileLoggerLayout").Value };
        //var logDiscord = new DiscordTarget() { LogChannelID = config.GetSection("LogChannel").Value, Layout = config.GetSection("BotLoggerLayout").Value };

        var loggerConfig = new LoggingConfiguration();
        loggerConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logConsole);
        loggerConfig.AddRule(LogLevel.Trace, LogLevel.Fatal, logFile);
        //loggerConfig.AddRule(LogLevel.Warn, LogLevel.Fatal, logDiscord);

        return loggerConfig;
    }

    private static IServiceProvider CreateProvider()
    {
        var botConfig = new DiscordConfiguration()
        {
            Token = token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
            LogUnknownEvents = false
        };

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)!.FullName)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build().GetSection("AppSettings");

        return new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton(botConfig)
            .AddSingleton<DiscordClient>()
            .AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddNLog();
            })
            .BuildServiceProvider();
    }

    public static void Main(string[] args) =>
        Program.MainAsync(args).GetAwaiter().GetResult();

    public static async Task MainAsync(string[] args)
    {
        LogManager.Configuration = LoggerConfig();

        var client = serviceProvider.GetRequiredService<DiscordClient>();
        client.Ready += OnClientConnect;

        await new CommandHandler(client, serviceProvider).InitializeAsync();
        await client.ConnectAsync();

        await Task.Delay(-1);
    }

    private static Task OnClientConnect(DiscordClient client, DSharpPlus.EventArgs.ReadyEventArgs args)
    {
        logger.Info($"Logged in as {client.CurrentUser.Username}#{client.CurrentUser.Discriminator}");

        return Task.CompletedTask;
    }
}