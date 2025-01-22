using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Targets;

namespace BotTemplate.Services;

public class DiscordTarget : TargetWithLayout
{
    private readonly DiscordClient Client = Program._serviceProvider.GetRequiredService<DiscordClient>();
    public string? LogChannelID { get; set; }
    private readonly Dictionary<LogLevel, DiscordColor> LogLevelColors = new Dictionary<LogLevel, DiscordColor>()
    {
        { LogLevel.Off, DiscordColor.Gray },
        { LogLevel.Debug, DiscordColor.Gray },
        { LogLevel.Trace, DiscordColor.Gray },
        { LogLevel.Info, DiscordColor.Green },
        { LogLevel.Warn, DiscordColor.Orange },
        { LogLevel.Error, DiscordColor.Red },
        { LogLevel.Fatal, DiscordColor.DarkRed },
    };

    protected override void Write(LogEventInfo LogEvent)
    {
        ulong ChannelID;
        bool Converted = ulong.TryParse(LogChannelID, out ChannelID);

        if (!Converted)
        {
            throw new ArgumentException($"Configuration for DiscordLogger was invalid; failed to convert {LogChannelID} to ulong.");
        }

        if (Client == null)
        {
            throw new ArgumentException($"Configuration for DiscordLogger was invalid; failed to get DiscordClient.");
        }

        var Embed = new DiscordEmbedBuilder()
        {
            Color = LogLevelColors[LogEvent.Level],
            Title = $"{LogEvent.Level}",
            Description = LogEvent.Message
        };

        Client.GetChannelAsync(ChannelID).Result.SendMessageAsync(Embed);
    }
}