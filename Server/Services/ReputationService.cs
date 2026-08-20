using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using TerritoryServer.Models;
using TerritoryServer.Servers;

namespace TerritoryServer.Loaders;

[Injectable(TypePriority = OnLoadOrder.TraderCallbacks - 1)]
public class ReputationService(StateServer stateServer, 
    DataConfig dataConfig,
    ProfileHelper profileHelper,
    ISptLogger<ReputationService> logger)
{
    public void CheckRep()
    {
        logger.Info("[TT] Checking profiles for missing reputation data.");
        
        Dictionary<MongoId, SptProfile> profiles = profileHelper.GetProfiles();

        foreach (SptProfile profile in profiles.Values)
        {
            CheckProfileRep(profile.CharacterData?.PmcData);
            CheckProfileRep(profile.CharacterData?.ScavData, true);
        }
        
        stateServer.SaveToDisk();
    }

    public void CheckRepForSession(MongoId sessionId)
    {
        SptProfile profile = profileHelper.GetFullProfile(sessionId);
        
        CheckProfileRep(profile.CharacterData?.PmcData);
        CheckProfileRep(profile.CharacterData?.ScavData, true);
        
        stateServer.SaveToDisk();
    }

    private void CheckProfileRep(PmcData? pmcData, bool scav = false)
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

            currentRep[factionName] = defaultRep;
        }
    }
}