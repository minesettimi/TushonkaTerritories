using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;
using TerritoryServer.Helpers;
using TerritoryServer.Models;
using TerritoryServer.Servers;
using TerritoryServer.Utils;

namespace TerritoryServer.Services;

[Injectable(InjectionType.Singleton)]
public class BattleService(
    ModConfig modConfig,
    DataConfig dataConfig,
    StateServer stateServer,
    LocationMapHelper mapHelper,
    MathUtil mathUtil,
    RandomUtil randomUtil)
{
    public static readonly List<string> MapList = 
    [
        "bigmap",
        "factory4_day",
        "interchange",
        "laboratory",
        "lighthouse",
        "reservbase",
        "sandbox",
        "shoreline",
        "tarkovstreets",
        "woods",
        "labyrinth",
        "suburbs",
        "terminal"
    ];
    
    private int _currentLocId;
    /*
     * Operate in stages:
     * 1. Spread to one nearby "none" locations next to current positions
     * 2. Run battle calculations if there's a battle
     * 3. Start contesting one nearby enemy locations if currently uncontested
     * Run these 3 stages per location
     * Uncap these if configured to
     * Run the configured number of locations per simulation
    */
    public void Simulate()
    {
        for (int i = 0; i < modConfig.BattleConfig.SimulationLocations; i++)
        {
            _currentLocId = TerritoryMath.Wrap(_currentLocId++, 0, MapList.Count);
            string currentLocation = MapList[_currentLocId];

            LocationState locState = stateServer.CurrentSave.Locations[currentLocation];
            
            if (locState.Holder == "none")
            {
                i--;
                continue;
            }
            
            SpreadNearby(currentLocation, locState);

            CalculateBattle(currentLocation, locState);

            if (locState.Contestants.Count == 1)
            {
                SpreadNearby(currentLocation, locState, false);
            }
        }
    }

    private void SpreadNearby(string location, LocationState locState, bool noneOnly = true)
    {
        string faction = locState.Holder;
        Faction factionData = dataConfig.Factions[faction];

        double distanceReduction = modConfig.BattleConfig.StrengthDecrease < 0
            ? modConfig.BattleConfig.StrengthDecrease
            : factionData.DistanceReduction;

        double moveStrength = locState.Contestants[faction] - distanceReduction;

        if (!(moveStrength > 0)) return;
        
        List<string> nearbyEmpty = FindNearby(location, noneOnly ? "none" : null);

        while (nearbyEmpty.Count > modConfig.BattleConfig.SimulationActions)
        {
            nearbyEmpty.RemoveAt(randomUtil.GetInt(0, nearbyEmpty.Count - 1));
        }

        foreach (string neighbor in nearbyEmpty)
        {
            LocationState emptyState = stateServer.CurrentSave.Locations[neighbor];

            emptyState.Holder = faction;
            emptyState.Contestants.Add(faction, moveStrength);
        }
    }

    //the goal is to calculate a battle between n contestants each with a strength 0.0 - 1.0 and defensiveness 0.0 - 1.0
    //each contestant will attack each other if not allies
    //spread out damage over all enemies
    //holder gets a strength buff
    //reduce all incoming damage once by the defensiveness
    //use percentage of location strength and faction strength to reduce values
    private void CalculateBattle(string locationName, LocationState location)
    {
        if (location.Contestants.Count < 2)
            return;

        Dictionary<string, double> damageDealt = [];
        
        foreach ((string contestant, double strength) in location.Contestants)
        {
            Faction factionData = dataConfig.Factions[contestant];

            List<string> targets = [];

            foreach (string other in location.Contestants.Keys)
            {
                if (contestant == other)
                    continue;

                if (factionData.Attitudes[other] == 1 || 
                    (factionData.Attitudes[other] == 0 && 
                     !randomUtil.GetChance100(modConfig.BattleConfig.AttackNeutralChance)))
                    continue;
                
                targets.Add(other);
            }

            double updatedStrength = strength;
            if (contestant == location.Holder)
            {
                double powerScalar = strength / factionData.Strength;
                updatedStrength += factionData.Defensiveness * powerScalar;
            }

            double damage =
                mathUtil.MapToRange((updatedStrength / targets.Count) * modConfig.BattleConfig.DamageMultiplier, 0.0,
                    2.0, 0.0, 1.0);

            damage += randomUtil.RandNum(modConfig.BattleConfig.DamageMinRng, modConfig.BattleConfig.DamageMaxRng);

            foreach (string target in targets)
            {
                damageDealt[target] += damage;
            }
        }

        foreach ((string faction, double damageTaken) in damageDealt)
        {
            if (!modConfig.BattleConfig.BaseTakingEnabled && faction == location.Holder && location.Base)
                continue;
            
            Faction factionData = dataConfig.Factions[faction];
            double powerScaling = location.Contestants[faction] / factionData.Strength;

            double defenseDecrease = factionData.Defensiveness * powerScaling;
            double finalDamage = Math.Clamp(damageTaken - defenseDecrease, 0.0, 1.0);

            location.Contestants[faction] -= finalDamage;

            if (!(location.Contestants[faction] < 0)) continue;
            
            location.Contestants.Remove(faction);

            foreach (string neighbor in dataConfig.LocationNeighbors[locationName])
            {
                RemoveIsolatedContestant(neighbor, faction);
            }
        }

        //figure out holder swapping
        
        if (location.Contestants.Count == 0)
        {
            location.Holder = "none";
            return;
        }
        
        if (location.Contestants.ContainsKey(location.Holder))
            return;

        double highestStrength = 0.0;
        string strongestFaction = "none";

        foreach ((string contestant, double strength) in location.Contestants)
        {
            if (!(strength > highestStrength)) continue;
            
            highestStrength = strength;
            strongestFaction = contestant;
        }

        location.Holder = strongestFaction;
    }

    //null target gets all nearby not of the same faction
    private List<string> FindNearby(string start, string? targetFaction)
    {
        List<string> results = [];

        LocationState currentLoc = stateServer.CurrentSave.Locations[start];
        
        List<string> neighbors = dataConfig.LocationNeighbors[start];
        foreach (string neighborLocation in neighbors)
        {
            string otherHolder = stateServer.CurrentSave.Locations[neighborLocation].Holder;

            if (targetFaction != null &&
                otherHolder == targetFaction || currentLoc.Holder != otherHolder)
            {
                results.Add(neighborLocation);
            }
        }

        return results;
    }

    private void RemoveIsolatedContestant(string location, string faction)
    {
        if (dataConfig.Factions[faction].Persistant)
            return;
        
        LocationState locState = stateServer.CurrentSave.Locations[location];

        if (locState.Holder == faction || !locState.Contestants.ContainsKey(faction))
            return;

        bool isolated = true;
        foreach (string neighbor in dataConfig.LocationNeighbors[location])
        {
            LocationState neighborState = stateServer.CurrentSave.Locations[neighbor];

            if (neighborState.Holder == faction)
            {
                isolated = false;
                return;
            }
        }

        if (isolated)
        {
            locState.Contestants.Remove(faction);
        }
    }
}