using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using TerritoryServer.Models;
using TerritoryServer.Servers;

namespace TerritoryServer.Loaders;

[Injectable(TypePriority = OnLoadOrder.SaveCallbacks + 50)]
public class ReputationService(StateServer stateServer, 
    DataConfig dataConfig,
    ProfileHelper profileHelper) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        Dictionary<MongoId, SptProfile> profiles = profileHelper.GetProfiles();

        foreach ((MongoId id, SptProfile profile) in profiles)
        {
            CheckProfileRep(profile.CharacterData?.PmcData);
            CheckProfileRep(profile.CharacterData?.ScavData, true);
        }
        
        return Task.CompletedTask;
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