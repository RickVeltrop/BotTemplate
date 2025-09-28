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
    private static readonly string? _token = Environment.GetEnvironmentVariable("TOKEN");
    public static readonly IServiceProvider _serviceProvider = CreateProvider();
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static LoggingConfiguration LoggerConfig()
    {
        var _config = _serviceProvider.GetRequiredService<IConfiguration>();

        var LogConsole = new ConsoleTarget("logconsole") { Layout = _config.GetSection("ConsoleLoggerLayout").Value };
        var LogFile = new FileTarget("logfile") { FileName = "${basedir}/logs/${shortdate}.log", Layout = _config.GetSection("FileLoggerLayout").Value };
        //var LogDiscord = new DiscordTarget() { LogChannelID = _config.GetSection("LogChannel").Value, Layout = _config.GetSection("BotLoggerLayout").Value };

        var config = new LoggingConfiguration();
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, LogConsole);
        config.AddRule(LogLevel.Trace, LogLevel.Fatal, LogFile);
        //config.AddRule(LogLevel.Warn, LogLevel.Fatal, LogDiscord);

        return config;
    }

    private static IServiceProvider CreateProvider()
    {
        var BotConfig = new DiscordConfiguration()
        {
            Token = _token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
            LogUnknownEvents = false
        };

        var Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)!.FullName)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build().GetSection("AppSettings");

        return new ServiceCollection()
            .AddSingleton<IConfiguration>(Configuration)
            .AddSingleton(BotConfig)
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
        var client = _serviceProvider.GetRequiredService<DiscordClient>();
        var config = _serviceProvider.GetRequiredService<IConfiguration>();

        LogManager.Configuration = LoggerConfig();

        client.Ready += OnClientConnect;

        await new CommandHandler(client, config).InitializeAsync();
        await client.ConnectAsync();

        await Task.Delay(-1);
    }

    private static Task OnClientConnect(DiscordClient Client, DSharpPlus.EventArgs.ReadyEventArgs Args)
    {
        _logger.Info($"Logged in as {Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}");

        return Task.CompletedTask;
    }
}