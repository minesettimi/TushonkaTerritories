using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using TerritoryServer.Models;
using TerritoryServer.Servers;

namespace TerritoryServer.Helpers;

[Injectable(InjectionType.Singleton)]
public class ReputationHelper(
    StateServer stateServer,
    DataConfig dataConfig,
    ModConfig modConfig,
    ISptLogger<ReputationHelper> logger)
{
    public void CheckProfileRep(PmcData? pmcData, bool scav = false)
    {
        if (pmcData == null || pmcData.Id == null)
            return;
        
        MongoId safeId = (MongoId)pmcData.Id;
        
        Dictionary<MongoId, Dictionary<string, double>> playerReps = stateServer.CurrentSave.PlayerRep;
        if (!stateServer.CurrentSave.PlayerRep.ContainsKey(safeId))
        {
            playerReps.TryAdd(safeId, []);
        }

        Dictionary<string, double> currentRep = playerReps[safeId];
        foreach ((string factionName, Faction faction) in dataConfig.Factions)
        {
            if (currentRep.ContainsKey(factionName))
                continue;

            double defaultRep;
            if (scav)
                defaultRep = faction.DefaultRepScav;
            else if (pmcData.Info?.Side is Sides.Bear)
                defaultRep = faction.DefaultRepBear;
            else
                defaultRep = faction.DefaultRepUsec;

            currentRep.Add(factionName, defaultRep);
        }

        if (modConfig.FactionConfig.TraderReputation)
        {
            UpdateTraderRep(pmcData);
        }
    }

    public void UpdateTraderRep(PmcData? pmcData)
    {
        if (pmcData?.TradersInfo == null)
            return;

        Dictionary<string, double>? playerRep = GetRep(pmcData);

        if (playerRep == null)
            return;

        foreach ((string factionName, Faction faction) in dataConfig.Factions)
        {
            if (factionName == "none" || faction.Trader == null)
                continue;

            MongoId traderId = (MongoId)faction.Trader;

            if (!pmcData.TradersInfo.ContainsKey(traderId))
            {
                logger.Error($"[TT] Faction: {factionName} has invalid trader id: {traderId}");
                continue;    
            }
            
            double rep = playerRep[factionName];
            pmcData.TradersInfo[traderId].Standing = rep;
        }
    }

    public void AddRepToProfile(PmcData? profile, string faction, double amount)
    {
        Dictionary<string, double>? playerRep = GetRep(profile);

        if (playerRep == null)
            return;
        
        if (!playerRep.ContainsKey(faction))
        {
            logger.Error($"[TT] Tried to give rep to invalid faction: {faction}!");
            return;
        }

        playerRep[faction] += amount;
    }
    
    private Dictionary<string, double>? GetRep(PmcData? profile)
    {
        if (profile == null || profile.Id == null)
            return null;
        
        MongoId safeId = (MongoId)profile.Id;

        return stateServer.CurrentSave.PlayerRep.GetValueOrDefault(safeId);
    }
}