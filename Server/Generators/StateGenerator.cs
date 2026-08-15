using SPTarkov.DI.Annotations;
using TerritoryServer.Helpers;
using TerritoryServer.Models;
using TerritoryServer.Services;

namespace TerritoryServer.Generators;

[Injectable(InjectionType.Singleton)]
public class StateGenerator(DataConfig dataConfig,
    LocationMapHelper mapHelper,
    ModConfig modConfig)
{
    public SaveState GenerateState()
    {
        SaveState newState = new();

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
            
            int distance = mapHelper.GetDistance(location, baseLocations[factionName], false);

            double newStrength = faction.Strength - faction.DistanceReduction * distance;

            //data config takes priority over simulationism
            if (newStrength < faction.DistanceReduction)
                newStrength = faction.DistanceReduction;

            LocationState locationState = new()
            {
                Holder = factionName,
                Base = false,
                Contestants =
                {
                    [factionName] = newStrength
                }
            };

            newState.Locations[factionName] = locationState;
        }

        return newState;
    }
}