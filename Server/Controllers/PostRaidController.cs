using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using TerritoryServer.Helpers;
using TerritoryServer.Models;
using TerritoryServer.Servers;
using TerritoryServer.Services;

namespace TerritoryServer.Controllers;

[Injectable(InjectionType.Singleton)]
public class PostRaidController(ProfileHelper profileHelper,
    StateServer stateServer,
    TerritoryModConfig modConfig,
    TerritoryDataConfig dataConfig,
    BattleService battleService,
    ProfileStateHelper profileStateHelper,
    CacheService cacheService,
    ISptLogger<PostRaidController> logger)
{
    public void PostRaidSimulate(string location, Dictionary<string, int> kills)
    {
        if (!modConfig.BattleConfig.BattlesEnabled || !modConfig.BattleConfig.RaidBattle)
        {
            return;
        }
        
        if (!modConfig.BattleConfig.RaidChangesBattle)
            battleService.Simulate();
        else
            battleService.Simulate(location, kills);
    }
    
    public void UpdateRaidReputation(string location, Dictionary<string, Dictionary<string, int>> kills)
    {
        if (!modConfig.FactionConfig.RepChange)
            return;

        LocationState? locationData = stateServer.CurrentLocationSave.Locations[location];
        if (locationData == null)
        {
            logger.Error("Failed to get location data for reputation changes at location id: " + location);
            return;
        }
        
        Dictionary<string, double> presentFactions = locationData.Contestants;
        FactionConfig factionConfig = modConfig.FactionConfig;
        foreach ((string player, Dictionary<string, int> playerKills) in kills)
        {
            PmcData? characterData;
            try
            {
                characterData = profileHelper.GetProfileByPmcId(player);
            }
            catch (Exception)
            {
                logger.Error($"Failed to get profile for player id: {player}.");
                continue;
            }

            if (characterData == null || characterData.Id == null)
            {
                logger.Error("[TT] Received raid completed request but profile is invalid!");
                return;
            }

            MongoId characterId = (MongoId)characterData.Id;
            if (!stateServer.CurrentPlayerSave.PlayerState.TryGetValue(characterId, out PlayerState? playerData))
            {
                logger.Error("[TT] Received raid completed request but profile doesn't have reputation data!");
                return;
            }

            foreach ((string botName, int amount) in playerKills)
            {
                string botFaction = cacheService.BotFactions.GetValueOrDefault(botName, "none");
                Faction faction = dataConfig.Factions[botFaction];
            
                if (botName == "none" 
                    || !playerData.Unlocked.TryGetValue(botFaction, out bool unlocked) 
                    || !unlocked
                    || (factionConfig.StaticRepGain && !presentFactions.ContainsKey(botFaction)))
                    continue;

                double repDecrease = factionConfig.KillReputationDecrease * amount;
                playerData.Reputation[botFaction] = Math.Max(playerData.Reputation[botFaction] - repDecrease, 0);

                double repIncrease = factionConfig.KillEnemyReputation * amount;
                foreach ((string otherFaction, int attitude) in faction.Attitudes)
                {
                    if (attitude != -1)
                        continue;
                
                    Faction otherFactionData =  dataConfig.Factions[otherFaction];
                    if (otherFactionData.Deactivated 
                        || !otherFactionData.RepEnabled
                        || (factionConfig.NonContestantRepGain && !presentFactions.ContainsKey(otherFaction)))
                        continue;
                    
                    playerData.Reputation[otherFaction] += repIncrease;
                }
            }

            stateServer.CurrentPlayerSave.PlayerState[characterId] = playerData;

            profileStateHelper.UpdateTraderRep(characterData);
        }

        
        stateServer.SaveToDisk();
    }
}