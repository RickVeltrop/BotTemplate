using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using NLog;
using System.Reflection;

namespace BotTemplate.Services;

public class CommandHandler
{
    private readonly Logger logger;
    private readonly SlashCommandsExtension slashCommandHandler;

    private Task RegisterCommandCategory(Type slashCommandClass)
    {
        var commandsGuildProperty = slashCommandClass.GetProperty("commandsGuild");
        ulong? commandsGuild = commandsGuildProperty?.PropertyType == typeof(ulong?) ? (ulong?)commandsGuildProperty.GetValue(null) : null;

        slashCommandHandler.RegisterCommands(slashCommandClass, commandsGuild);

        return Task.CompletedTask;
    }

    private Task OnSlashCmdError(SlashCommandsExtension sender, SlashCommandErrorEventArgs args)
    {
        logger.Error($"Uncaught exception {args.Exception.GetType().Name} in /{args.Context.QualifiedName}: ```\n{args.Exception}\n```");

        args.Context.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"An uncaught exception occured while running this command."));

        return Task.CompletedTask;
    }

    public CommandHandler(DiscordClient client, IServiceProvider serviceProvider)
    {
        logger = LogManager.GetCurrentClassLogger();

        slashCommandHandler = client.UseSlashCommands(new SlashCommandsConfiguration
        {
            Services = serviceProvider
        });
        slashCommandHandler.SlashCommandErrored += OnSlashCmdError;
    }

    public async Task InitializeAsync()
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
        {
            logger.Error("[Commands/Initialization] Could not load commands due to Assembly.GetEntryAssembly() returning a null value.");
            return;
        }

        logger.Info("[Commands/Initialization] Loading commands");
        var commandCategories = assembly.GetTypes().Where(Type => Type.IsClass && typeof(ApplicationCommandModule).IsAssignableFrom(Type));
        if (!commandCategories.Any())
            logger.Warn("[Commands/Initialization] No command categories were found");

        foreach (var category in commandCategories)
        {
            try
            {
                await RegisterCommandCategory(category);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        logger.Info("[Commands/Initialization] Loaded commands");
    }
}