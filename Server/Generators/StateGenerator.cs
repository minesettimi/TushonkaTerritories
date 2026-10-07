using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Mod;
using TerritoryServer.Helpers;
using TerritoryServer.Models;
using TerritoryServer.Services;

namespace TerritoryServer.Generators;

[Injectable(InjectionType.Singleton)]
public class StateGenerator(DataConfig dataConfig,
    LocationMapHelper mapHelper,
    TerritoryModConfig modConfig,
    IReadOnlyList<SptMod> modList,
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
            if (faction.Deactivated || faction.Base == null || faction.Base == "none")
                continue;
            
            baseLocations[factionName] = faction.Base;
        }
        
        foreach (string location in LocationService.MapList)
        {
            if (newState.Locations[location] != null)
                continue;

            LocationInitialState initialState = dataConfig.LocationTerritories[location];
            string factionName = initialState.Holder;
            
            Faction faction = dataConfig.Factions[factionName];

            if (faction.Deactivated)
            {
                factionName = faction.ModSupport?.BackupFaction ?? "none";
                faction = dataConfig.Factions[factionName];
            }
            
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

            bool locked = initialState.Locked;
            if (initialState.ModLock != null)
            {
                locked = modList.All(mod => mod.ModMetadata.ModGuid != initialState.ModLock);
            }

            LocationState locationState = new()
            {
                Holder = factionName,
                Base = baseLocations.ContainsKey(factionName),
                Locked = locked,
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