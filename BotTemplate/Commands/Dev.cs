using BotTemplate.Modals;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using NLog;

namespace BotTemplate.Commands;

[RequireUserId(519508374581149707)]
[SlashCommandGroup("dev", "Development commands.")]
public class Dev : ApplicationCommandModule
{
    public static ulong? commandsGuild { get; set; }

    public enum levels
    {
        Trace, Debug, Info, Warn, Error, Fatal, Off
    };

    private readonly Logger logger;

    public Dev()
    {
        logger = LogManager.GetCurrentClassLogger();
    }

    [SlashCommand("log", "Send a message with specified loglevel to the logger.")]
    public async Task Log(
        InteractionContext ctx,
        [Option("LogLevel", "The log level")] levels lvl,
        [Option("Message", "The log message")] string message
        )
    {
        await ctx.DeferAsync(false);

        LogLevel level = LogLevel.FromOrdinal((int)lvl);
        logger.Log(level, message);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Successfully sent {level.Name} message {message}!"));
    }

    [SlashCommand("clearcommands", "Clears all the bot's commands and restarts the bot.")]
    public async Task ClearCommands(InteractionContext ctx, [Option("GuildId", "The guild to clear the commands of.")] string guildIDStr = "")
    {
        await ctx.DeferAsync(false);

        if (guildIDStr == "")
        {
            logger.Info("Removing old commands");
            foreach (DiscordApplicationCommand Command in ctx.Client.GetGlobalApplicationCommandsAsync().Result)
            {
                await ctx.Client.DeleteGlobalApplicationCommandAsync(Command.Id);
            }
        }
        else if (guildIDStr != "")
        {
            ulong guildID;
            bool converted = ulong.TryParse(guildIDStr, out guildID);

            if (!converted)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Failed to convert {guildIDStr} to ulong."));
                return;
            }

            logger.Info($"Removing old commands from guild with ID {guildID}");
            foreach (DiscordApplicationCommand Command in ctx.Client.GetGuildApplicationCommandsAsync(guildID).Result)
            {
                await ctx.Client.DeleteGuildApplicationCommandAsync(guildID, Command.Id);
            }
        }

        System.Diagnostics.Process.Start(Environment.ProcessPath!);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"The bot is will be restarted!"));
        logger.Warn($"<@{ctx.User.Id}>({ctx.User.Id}) used `/{ctx.QualifiedName}` {(guildIDStr == "" ? "" : $"with ID `{guildIDStr}`")}");

        Environment.Exit(0);
    }

    [SlashCommand("restart", "Restarts the bot.")]
    public async Task Restart(InteractionContext ctx)
    {
        await ctx.DeferAsync(false);

        System.Diagnostics.Process.Start(Environment.ProcessPath!);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Restarting!"));
        logger.Warn($"<@{ctx.User.Id}>({ctx.User.Id}) used `/{ctx.QualifiedName}`!");

        Environment.Exit(0);
    }

    [SlashCommand("shutdown", "Shuts down the bot.")]
    public async Task Shutdown(InteractionContext ctx)
    {
        await ctx.DeferAsync(false);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Shutting down!"));
        logger.Warn($"<@{ctx.User.Id}>({ctx.User.Id}) used `/{ctx.QualifiedName}`!");

        Environment.Exit(0);
    }
}