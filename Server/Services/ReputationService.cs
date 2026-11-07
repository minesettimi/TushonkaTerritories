using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using TerritoryServer.Helpers;
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
            CheckProfile(profile);
        }
        
        stateServer.SaveToDisk();
    }

    public void CheckRepForSession(MongoId sessionId)
    {
        SptProfile profile = profileHelper.GetFullProfile(sessionId);
        CheckProfile(profile);
        
        stateServer.SaveToDisk();
    }

    private void CheckProfile(SptProfile profile)
    {
        profileStateHelper.CheckProfileData(profile.CharacterData?.PmcData);
        profileStateHelper.CheckProfileData(profile.CharacterData?.ScavData, true);
    }
}