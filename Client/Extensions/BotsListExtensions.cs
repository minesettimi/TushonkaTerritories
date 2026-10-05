using System.Collections.Generic;
using EFT;

namespace TerritoryClient.Extensions;

public static class BotsListExtensions
{
    public static int GetBotCountByFaction(this BotsList botsList, string factionName)
    {
        int botCount = 0;
        
        foreach (BotOwner bot in botsList._botOwners)
        {
            if (!TerritoryPlugin.StateManager.ServerData.BotFaction.TryGetValue(bot.Settings._role.ToString(),
                    out string botFaction))
            {
                continue;
            }
            
            if (botFaction == factionName)
                botCount++;
        }

        return botCount;
    }
}