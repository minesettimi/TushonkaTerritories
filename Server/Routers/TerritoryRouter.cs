using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;
using TerritoryServer.Controllers;
using TerritoryServer.Models;
using TerritoryServer.Services;

namespace TerritoryServer.Routers;

[Injectable(TypePriority = OnLoadOrder.Routers + 1)]
public class TerritoryRouter(JsonUtil jsonUtil, TerritoryCallbacks territoryCallbacks) : StaticRouter(jsonUtil, [
    new RouteAction<RaidStatRequest>(
        "/tt/match/end",
        async (
            url,
            info,
            sessionId,
            output,
            cancellationToken
            ) => await territoryCallbacks.HandleMatchEnd(info)
        )
])
{ }

[Injectable]
public class TerritoryCallbacks(HttpResponseUtil httpResponseUtil, 
    PostRaidController raidController,
    BattleService battleService)
{
    public ValueTask<string> HandleMatchEnd(RaidStatRequest statRequest)
    {
        raidController.UpdateRaidReputation(statRequest.PlayerKills,  statRequest.Scav);
        raidController.PostRaidSimulate(statRequest.Location, statRequest.Kills);
        
        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }
}