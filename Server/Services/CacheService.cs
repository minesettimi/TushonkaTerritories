using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using TerritoryServer.Models;

namespace TerritoryServer.Services;

[Injectable(InjectionType.Singleton)]
public class CacheService(DataConfig dataConfig,
    TerritoryModConfig modConfig,
    IReadOnlyList<SptMod> modList,
    ISptLogger<CacheService> logger)
{
    public readonly Dictionary<string, string> BotFactions = [];
    public readonly HashSet<string> ValidFactions = [];
    public readonly HashSet<string> PhantomFactions = [];
    
    public void Initialize()
    {
        CacheModdedFactions(); //first
        CacheFactions();
    }

    private void CacheFactions()
    {
        foreach ((string factionName, Faction faction) in dataConfig.Factions)
        {
            if (faction.Deactivated)
                continue;
            
            //there's a lot of separate bot lists but all are pretty necessary
            IEnumerable<string> botNames = faction.MobileBossNames.Concat(faction.BotNames).Concat(faction.ExtraBotNames)
                .Concat(faction.ExtraBossNames);
            foreach (string botName in botNames)
            {
                if (!BotFactions.TryAdd(botName, factionName))
                {
                    throw new Exception($"[TT] Bot with name {botName} exists in multiple factions!");
                }
            }
        }
        
        //get remaining
        foreach (WildSpawnType botId in Enum.GetValues<WildSpawnType>())
        {
            BotFactions.TryAdd(botId.ToString(), "none");
        }
    }

    private void CacheModdedFactions()
    {
        foreach ((string factionName, Faction faction) in dataConfig.Factions)
        {
            if (faction.ModSupport == null 
                || modList.Any(mod => mod.ModMetadata.ModGuid == faction.ModSupport.ModGuid))
            {
                ValidFactions.Add(factionName);

                if (faction.Phantom)
                    PhantomFactions.Add(factionName);
                
                continue;
            }

            faction.Deactivated = true;
            
            if (modConfig.Debug && factionName != "none")
            {
                logger.Info($"[TT] Deactivated faction: {factionName} which requires mod: {faction.ModSupport?.ModGuid}");   
            }
        }
    }
}