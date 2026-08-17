using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using TerritoryServer.Services;

namespace TerritoryServer.Loaders;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 100000)]
public class PostLoad(LocationService locationService, ReputationService reputationService,
    BattleService battleService) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        locationService.Initialize();
        reputationService.CheckRep();
        
        //test
        battleService.Simulate();

        return Task.CompletedTask;
    }
}