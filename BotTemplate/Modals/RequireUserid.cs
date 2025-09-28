using DSharpPlus.SlashCommands;

namespace BotTemplate.Modals;

public class RequireUserIdAttribute : SlashCheckBaseAttribute
{
    private readonly ulong userID;

    public RequireUserIdAttribute(ulong userID)
    {
        this.userID = userID;
    }

    public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    {
        if (ctx.User.Id == userID)
            return true;
        else
            return false;
    }
}