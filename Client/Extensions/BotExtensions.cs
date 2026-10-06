using System.Collections.Generic;
using EFT;

namespace TerritoryClient.Extensions;

public static class BotExtensions
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

    public static BotZone? GetRandomBotZoneExclusive(this BotSpawner botSpawner, bool sniper)
    {
        List<BotZone> zones = [];
        foreach (BotZone zone in botSpawner._openedZones)
        {
            if (sniper ^ zone.SnipeZone)
                continue;
            
            zones.Add(zone);
        }

        return zones.Count == 0 ? null : zones.RandomElement();
    }
}