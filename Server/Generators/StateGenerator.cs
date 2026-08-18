using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using TerritoryServer.Helpers;
using TerritoryServer.Models;
using TerritoryServer.Services;

namespace TerritoryServer.Generators;

[Injectable(InjectionType.Singleton)]
public class StateGenerator(DataConfig dataConfig,
    LocationMapHelper mapHelper,
    ModConfig modConfig,
    ISptLogger<StateGenerator> logger)
{
    public SaveState GenerateState()
    {
        SaveState newState = new()
        {
            StateId = new MongoId()
        };

        Dictionary<string, string> baseLocations = [];
        
        foreach ((string factionName, Faction faction) in dataConfig.Factions)
        {
            if (faction.Base == null || faction.Base == "none")
                continue;

            LocationState locationState = new()
            {
                Holder = factionName,
                Base = true,
                Contestants =
                {
                    [factionName] = faction.Strength
                }
            };

            baseLocations[factionName] = faction.Base;

            newState.Locations[faction.Base] = locationState;
        }
        
        foreach (string location in LocationService.MapList)
        {
            if (newState.Locations[location] != null)
                continue;

            string factionName = dataConfig.LocationTerritories[location];
            Faction faction = dataConfig.Factions[factionName];
            
            int distance = mapHelper.GetDistance(location, baseLocations.GetValueOrDefault(factionName, location), false);

            if (distance == -1)
                distance = 1;

            double distanceReduction = modConfig.BattleConfig.StrengthDecrease < 0
                ? faction.DistanceReduction
                : modConfig.BattleConfig.StrengthDecrease;
            
            double newStrength = faction.Strength - distanceReduction * distance;

            //data config takes priority over simulationism
            if (newStrength < distanceReduction)
                newStrength = distanceReduction;

            LocationState locationState = new()
            {
                Holder = factionName,
                Base = false,
                Contestants =
                {
                    [factionName] = newStrength
                }
            };

            newState.Locations[location] = locationState;
        }

        logger.Info("[TT] Completed save generation.");
        
        return newState;
    }
}