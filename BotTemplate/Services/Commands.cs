using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System.Reflection;

namespace BotTemplate.Services;

public class CommandHandler
{
    private readonly Logger _logger;
    private readonly IConfiguration _config;
    private readonly DiscordClient _client;
    private readonly SlashCommandsExtension SlashCmdHandler;

    private Task RegisterCommandCategory(Type SlashCommandClass)
    {
        var CommandsGuildProperty = SlashCommandClass.GetProperty("CommandsGuild");
        ulong? CommandsGuild = CommandsGuildProperty?.PropertyType == typeof(ulong?) ? (ulong?)CommandsGuildProperty.GetValue(null) : null;

        SlashCmdHandler.RegisterCommands(SlashCommandClass, CommandsGuild);

        return Task.CompletedTask;
    }

    private Task OnSlashCmdError(SlashCommandsExtension Sender, SlashCommandErrorEventArgs Args)
    {
        _logger.Error($"Uncaught exception {Args.Exception.GetType().Name} in /{Args.Context.QualifiedName}: ```\n{Args.Exception}\n```");

        Args.Context.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"An uncaught exception occured while running this command."));

        return Task.CompletedTask;
    }

    public CommandHandler(DiscordClient Client, IConfiguration Config)
    {
        _logger = LogManager.GetCurrentClassLogger();
        _config = Config;
        _client = Client;

        SlashCmdHandler = Client.UseSlashCommands(new SlashCommandsConfiguration
        {
            Services = new ServiceCollection()
            .AddSingleton(Config)
            .BuildServiceProvider()
        });
        SlashCmdHandler.SlashCommandErrored += OnSlashCmdError;
    }

    public async Task InitializeAsync()
    {
        var Asm = Assembly.GetEntryAssembly();
        if (Asm == null)
        {
            _logger.Error("[Commands/Initialization] Could not load commands due to Assembly.GetEntryAssembly() returning a null value.");
            return;
        }

        _logger.Info("[Commands/Initialization] Loading commands");
        var CmdCategories = Asm.GetTypes().Where(Type => Type.IsClass && typeof(ApplicationCommandModule).IsAssignableFrom(Type));
        if (!CmdCategories.Any())
            _logger.Warn("[Commands/Initialization] No command categories were found");

        foreach (var Category in CmdCategories)
        {
            try
            {
                await RegisterCommandCategory(Category);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }

        _logger.Info("[Commands/Initialization] Loaded commands");
    }
}