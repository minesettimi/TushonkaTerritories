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
#pragma warning disable CS0612 // Type or member is obsolete

namespace TerritoryServer.Loaders;

[Injectable(InjectionType.Singleton)]
public class ReputationService(StateServer stateServer, 
    ProfileStateHelper profileStateHelper,
    ProfileHelper profileHelper,
    ISptLogger<ReputationService> logger)
{
    public void CheckRep()
    {
        logger.Info("[TT] Checking profiles for missing reputation data.");
        
        Dictionary<MongoId, SptProfile> profiles = profileHelper.GetProfiles();

        foreach (SptProfile profile in profiles.Values)
        {
            profileStateHelper.CheckProfileData(profile.CharacterData?.PmcData);
            profileStateHelper.CheckProfileData(profile.CharacterData?.ScavData, true);
        }
        
        stateServer.CurrentSave.PlayerRep = null;
        
        stateServer.SaveToDisk();
    }

    public void CheckRepForSession(MongoId sessionId)
    {
        SptProfile profile = profileHelper.GetFullProfile(sessionId);
        
        profileStateHelper.CheckProfileData(profile.CharacterData?.PmcData);
        profileStateHelper.CheckProfileData(profile.CharacterData?.ScavData, true);
        
        stateServer.SaveToDisk();
    }
}