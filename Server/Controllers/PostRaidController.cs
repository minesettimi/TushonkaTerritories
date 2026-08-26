using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using TerritoryServer.Loaders;
using TerritoryServer.Models;
using TerritoryServer.Servers;
using TerritoryServer.Services;

namespace TerritoryServer.Controllers;

[Injectable(InjectionType.Singleton)]
public class PostRaidController(ProfileHelper profileHelper,
    StateServer stateServer,
    ModConfig modConfig,
    DataConfig dataConfig,
    BattleService battleService,
    ReputationService reputationService,
    ISptLogger<PostRaidController> logger)
{
    public void PostRaidSimulate(string location, Dictionary<string, int> kills)
    {
        if (!modConfig.BattleConfig.BattlesEnabled || !modConfig.BattleConfig.RaidBattle)
        {
            stateServer.SaveToDisk();
            return;
        }
        
        if (!modConfig.BattleConfig.RaidChangesBattle)
            battleService.Simulate();
        else
            battleService.Simulate(location, kills);
    }
    
    public void UpdateRaidReputation(Dictionary<string, Dictionary<string, int>> kills, bool scav)
    {
        if (modConfig.Debug)
        {
            logger.Info($"Player kills: {JsonSerializer.Serialize(kills)}");
        }
        
        if (!modConfig.FactionConfig.RepChange)
            return;
        
        FactionConfig factionConfig = modConfig.FactionConfig;

        foreach ((string player, Dictionary<string, int> playerKills) in kills)
        {
            PmcData? characterData;
            try
            {
                characterData = profileHelper.GetProfileByPmcId(player);
            }
            catch (Exception e)
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
            if (!stateServer.CurrentSave.PlayerRep.TryGetValue(characterId, out Dictionary<string, double>? reputation))
            {
                logger.Error("[TT] Received raid completed request but profile doesn't have reputation data!");
                return;
            }

            foreach ((string botName, int amount) in playerKills)
            {
                string botFaction = dataConfig.BotFaction.GetValueOrDefault(botName, "none");
                Faction faction = dataConfig.Factions[botFaction];
            
                if (!faction.RepEnabled)
                    continue;

                double repDecrease = factionConfig.KillReputationDecrease * amount;
                reputation[botFaction] = Math.Min(reputation[botFaction] - repDecrease, 0);

                double repIncrease = factionConfig.KillEnemyReputation * amount;
                foreach ((string otherFaction, int attitude) in faction.Attitudes)
                {
                    if (attitude != -1)
                        continue;
                
                    Faction otherFactionData =  dataConfig.Factions[otherFaction];

                    if (!otherFactionData.RepEnabled)
                        continue;

                    reputation[otherFaction] += repIncrease;
                }
            }

            stateServer.CurrentSave.PlayerRep[characterId] = reputation;

            if (modConfig.FactionConfig.TraderReputation && !scav)
            {
                reputationService.UpdateTraderRep(characterData);
            }
        }

    }
}