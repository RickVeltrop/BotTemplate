using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Targets;

namespace BotTemplate.Services;

public class DiscordTarget : TargetWithLayout
{
    private readonly DiscordClient client = Program.serviceProvider.GetRequiredService<DiscordClient>();
    public string? logChannelID { get; set; }
    private readonly Dictionary<LogLevel, DiscordColor> logLevelColors = new Dictionary<LogLevel, DiscordColor>()
    {
        { LogLevel.Off, DiscordColor.Gray },
        { LogLevel.Debug, DiscordColor.Gray },
        { LogLevel.Trace, DiscordColor.Gray },
        { LogLevel.Info, DiscordColor.Green },
        { LogLevel.Warn, DiscordColor.Orange },
        { LogLevel.Error, DiscordColor.Red },
        { LogLevel.Fatal, DiscordColor.DarkRed },
    };

    protected override void Write(LogEventInfo logEvent)
    {
        ulong channelID;
        bool converted = ulong.TryParse(logChannelID, out channelID);

        if (!converted)
        {
            throw new ArgumentException($"Configuration for DiscordLogger was invalid; failed to convert {logChannelID} to ulong.");
        }

        if (client == null)
        {
            throw new ArgumentException($"Configuration for DiscordLogger was invalid; failed to get DiscordClient.");
        }

        var Embed = new DiscordEmbedBuilder()
        {
            Color = logLevelColors[logEvent.Level],
            Title = $"{logEvent.Level}",
            Description = logEvent.Message
        };

        client.GetChannelAsync(channelID).Result.SendMessageAsync(Embed);
    }
}