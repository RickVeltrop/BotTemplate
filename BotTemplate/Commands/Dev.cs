using BotTemplate.Modals;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using NLog;

namespace BotTemplate.Commands;

[RequireUserId(0)]
[SlashCommandGroup("dev", "Development commands.")]
public class Dev : ApplicationCommandModule
{
    public static ulong? CommandsGuild { get; set; }

    public enum Levels
    {
        Trace, Debug, Info, Warn, Error, Fatal, Off
    };

    private readonly Logger _logger;

    public Dev()
    {
        _logger = LogManager.GetCurrentClassLogger();
    }

    [SlashCommand("log", "Send a message with specified loglevel to the logger.")]
    public async Task Log(
        InteractionContext ctx,
        [Option("LogLevel", "The log level")] Levels lvl,
        [Option("Message", "The log message")] string msg
        )
    {
        await ctx.DeferAsync(false);

        LogLevel Level = LogLevel.FromOrdinal((int)lvl);
        _logger.Log(Level, msg);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Successfully sent {Level.Name} message {msg}!"));
    }

    [SlashCommand("clearcommands", "Clears all the bot's commands and restarts the bot.")]
    public async Task ClearCommands(InteractionContext ctx, [Option("GuildId", "The guild to clear the commands of.")] string GuildIdStr = "")
    {
        await ctx.DeferAsync(false);

        if (GuildIdStr == "")
        {
            _logger.Info("Removing old commands");
            foreach (DiscordApplicationCommand Command in ctx.Client.GetGlobalApplicationCommandsAsync().Result)
            {
                await ctx.Client.DeleteGlobalApplicationCommandAsync(Command.Id);
            }
        }
        else if (GuildIdStr != "")
        {
            ulong GuildId;
            bool Converted = ulong.TryParse(GuildIdStr, out GuildId);

            if (!Converted)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Failed to convert {GuildIdStr} to ulong."));
                return;
            }

            _logger.Info($"Removing old commands from guild with ID {GuildId}");
            foreach (DiscordApplicationCommand Command in ctx.Client.GetGuildApplicationCommandsAsync(GuildId).Result)
            {
                await ctx.Client.DeleteGuildApplicationCommandAsync(GuildId, Command.Id);
            }
        }

        System.Diagnostics.Process.Start(Environment.ProcessPath!);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"The bot is being restarted!"));
        _logger.Warn($"<@{ctx.User.Id}>({ctx.User.Id}) used `/{ctx.QualifiedName}` {(GuildIdStr == "" ? "" : $"with ID `{GuildIdStr}`")}");

        Environment.Exit(0);
    }

    [SlashCommand("restart", "Restarts the bot.")]
    public async Task Restart(InteractionContext ctx)
    {
        await ctx.DeferAsync(false);

        System.Diagnostics.Process.Start(Environment.ProcessPath!);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Restarting!"));
        _logger.Warn($"<@{ctx.User.Id}>({ctx.User.Id}) used `/{ctx.QualifiedName}`!");

        Environment.Exit(0);
    }

    [SlashCommand("shutdown", "Shuts down the bot.")]
    public async Task ShutDown(InteractionContext ctx)
    {
        await ctx.DeferAsync(false);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Shutting down!"));
        _logger.Warn($"<@{ctx.User.Id}>({ctx.User.Id}) used `/{ctx.QualifiedName}`!");

        Environment.Exit(0);
    }
}