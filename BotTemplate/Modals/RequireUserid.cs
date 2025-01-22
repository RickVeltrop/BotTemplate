using DSharpPlus.SlashCommands;

namespace BotTemplate.Modals;

public class RequireUserIdAttribute : SlashCheckBaseAttribute
{
    private readonly ulong UserId;

    public RequireUserIdAttribute(ulong userId)
    {
        this.UserId = userId;
    }

    public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    {
        if (ctx.User.Id == UserId)
            return true;
        else
            return false;
    }
}