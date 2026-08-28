using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils.Cloners;
using TerritoryServer.Models;
using TerritoryServer.Servers;
#pragma warning disable CS0612 // Type or member is obsolete

namespace TerritoryServer.Helpers;

[Injectable(InjectionType.Singleton)]
public class ProfileStateHelper(
    StateServer stateServer,
    DataConfig dataConfig,
    ModConfig modConfig,
    ICloner cloner,
    ISptLogger<ProfileStateHelper> logger)
{
    public void CheckProfileData(PmcData? pmcData, bool scav = false)
    {
        if (pmcData == null || pmcData.Id == null)
            return;
        
        MongoId safeId = (MongoId)pmcData.Id;
        
        Dictionary<MongoId, PlayerState> playerStates = stateServer.CurrentSave.PlayerState;
        if (!stateServer.CurrentSave.PlayerState.ContainsKey(safeId))
        {
            PlayerState newState = new()
            {
                Unlocked = [],
                Reputation = []
            };

            //migrate data
            if (stateServer.CurrentSave.PlayerRep != null &&
                stateServer.CurrentSave.PlayerRep.TryGetValue(safeId, out Dictionary<string, double>? reputation))
            {
                newState.Reputation = cloner.Clone(reputation)!;
            }
            
            playerStates.TryAdd(safeId, newState);
        }

        Dictionary<string, double> currentRep = playerStates[safeId].Reputation;
        Dictionary<string, bool> currentUnlocked = playerStates[safeId].Unlocked;
        
        foreach ((string factionName, Faction faction) in dataConfig.Factions)
        {
            if (!currentRep.ContainsKey(factionName))
            {
                double defaultRep;
                if (scav)
                    defaultRep = faction.DefaultRepScav;
                else if (pmcData.Info?.Side is Sides.Bear)
                    defaultRep = faction.DefaultRepBear;
                else
                    defaultRep = faction.DefaultRepUsec;

                currentRep.Add(factionName, defaultRep);
            }

            if (!currentUnlocked.ContainsKey(factionName))
            {
                currentUnlocked[factionName] = faction.RepEnabled;
            }
        }

        playerStates[safeId].Reputation = currentRep;
        playerStates[safeId].Unlocked = currentUnlocked;
        stateServer.CurrentSave.PlayerState = playerStates;

        if (modConfig.FactionConfig.TraderReputation)
        {
            UpdateTraderRep(pmcData);
        }
    }

    public void UpdateTraderRep(PmcData? pmcData)
    {
        if (pmcData?.TradersInfo == null)
            return;

        Dictionary<string, double>? playerRep = GetState(pmcData)?.Reputation;

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
        PlayerState? state = GetState(profile);
        Dictionary<string, double>? playerRep = state?.Reputation;

        if (playerRep == null)
            return;
        
        if (!playerRep.ContainsKey(faction))
        {
            logger.Error($"[TT] Tried to give rep to invalid faction: {faction}!");
            return;
        }

        state!.Reputation[faction] += amount;
    }

    public void SetFactionLock(PmcData? profile, string faction, bool locked = false)
    {
        Dictionary<string, bool>? unlocked = GetState(profile)?.Unlocked;

        if (unlocked == null)
            return;
        
        if (!unlocked.ContainsKey(faction))
        {
            logger.Error($"[TT] Tried to set locked value to invalid faction: {faction}!");
            return;
        }

        unlocked[faction] = !locked;
    }
    
    private PlayerState? GetState(PmcData? profile)
    {
        if (profile == null || profile.Id == null)
            return null;
        
        MongoId safeId = (MongoId)profile.Id;

        return stateServer.CurrentSave.PlayerState.GetValueOrDefault(safeId);
    }
}