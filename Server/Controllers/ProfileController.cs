using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using TerritoryServer.Models;
using TerritoryServer.Servers;

namespace TerritoryServer.Controllers;

[Injectable(InjectionType.Singleton)]
public class ProfileController(StateServer stateServer,
    TerritoryDataConfig dataConfig,
    ProfileHelper profileHelper)
{
    public void ApplyRepPreset(string preset, MongoId sessionId)
    {
        PmcData? profile = profileHelper.GetPmcProfile(sessionId);
    }
}