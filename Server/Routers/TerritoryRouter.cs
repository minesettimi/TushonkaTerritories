using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;
using TerritoryServer.Controllers;
using TerritoryServer.Models;
using TerritoryServer.Servers;

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
        ),
    new RouteAction<EmptyRequestData>(
        "/tt/state/data",
        async (
            url,
            info,
            sessionId,
            output,
            cancellationToken
        ) => await territoryCallbacks.HandleDataRetrieval()
    ),
    new RouteAction<EmptyRequestData>(
        "/tt/state/server",
        async (
            url,
            info,
            sessionId,
            output,
            cancellationToken
        ) => await territoryCallbacks.HandleStateRetrieval()
    )
])
{ }

[Injectable]
public class TerritoryCallbacks(HttpResponseUtil httpResponseUtil, 
    PostRaidController raidController,
    StateServer stateServer,
    ModConfig modConfig,
    DataConfig dataConfig)
{
    public ValueTask<string> HandleMatchEnd(RaidStatRequest statRequest)
    {
        raidController.UpdateRaidReputation(statRequest.PlayerKills);
        raidController.PostRaidSimulate(statRequest.Location, statRequest.Kills);
        
        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }

    public ValueTask<string> HandleDataRetrieval()
    {
        ServerDataResponse dataResponse = new()
        {
            BotFaction = dataConfig.BotFaction,
            AttitudeEffect = modConfig.RaidConfig.AttitudeEffectPlayer,
            AllyRep = modConfig.RaidConfig.AllyRep,
            NeutralRep = modConfig.RaidConfig.NeutralRep,
            FactionColors = [],
            ContinualUpdates = modConfig.BattleConfig.SimulationInterval
        };

        foreach ((string factionName, Faction faction) in dataConfig.Factions)
        {
            dataResponse.FactionColors.Add(factionName, faction.Color);
        }

        return new ValueTask<string>(httpResponseUtil.NoBody(dataResponse));
    }
    
    public ValueTask<string> HandleStateRetrieval()
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(stateServer.CurrentSave));
    }
}