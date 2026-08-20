using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Services.Profile;
using TerritoryServer.Loaders;

namespace TerritoryServer.Overrides;

[Injectable]
public class CreateProfileOverride : AbstractPatch
{
    public static ReputationService ReputationService = null!;
    
    public CreateProfileOverride(ReputationService reputationService)
    {
        ReputationService = reputationService;
    }
    
    protected override MethodBase? GetTargetMethod()
    {
        return typeof(CreateProfileService).GetMethod(nameof(CreateProfileService.CreateProfile));
    }

    [PatchPostfix]
    public static void Postfix(MongoId sessionId)
    {
        ReputationService.CheckRepForSession(sessionId);
    }
}