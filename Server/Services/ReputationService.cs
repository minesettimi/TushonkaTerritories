using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using TerritoryServer.Helpers;
using TerritoryServer.Models;
using TerritoryServer.Servers;

namespace TerritoryServer.Loaders;

[Injectable(InjectionType.Singleton)]
public class ReputationService(StateServer stateServer, 
    ReputationHelper reputationHelper,
    ProfileHelper profileHelper,
    ISptLogger<ReputationService> logger)
{
    public void CheckRep()
    {
        logger.Info("[TT] Checking profiles for missing reputation data.");
        
        Dictionary<MongoId, SptProfile> profiles = profileHelper.GetProfiles();

        foreach (SptProfile profile in profiles.Values)
        {
            reputationHelper.CheckProfileRep(profile.CharacterData?.PmcData);
            reputationHelper.CheckProfileRep(profile.CharacterData?.ScavData, true);
        }
        
        stateServer.SaveToDisk();
    }

    public void CheckRepForSession(MongoId sessionId)
    {
        SptProfile profile = profileHelper.GetFullProfile(sessionId);
        
        reputationHelper.CheckProfileRep(profile.CharacterData?.PmcData);
        reputationHelper.CheckProfileRep(profile.CharacterData?.ScavData, true);
        
        stateServer.SaveToDisk();
    }
}