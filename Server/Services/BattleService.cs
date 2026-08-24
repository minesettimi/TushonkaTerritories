using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;
using TerritoryServer.Models;
using TerritoryServer.Servers;
using TerritoryServer.Utils;

namespace TerritoryServer.Services;

[Injectable(InjectionType.Singleton)]
public class BattleService(
    ModConfig modConfig,
    DataConfig dataConfig,
    StateServer stateServer,
    RandomUtil randomUtil,
    LocationService locationService,
    ISptLogger<BattleService> logger)
{
    private Timer _battleTimer;

    private enum RaidBattleState
    {
        Waiting,
        Blocking,
        Complete
    }
    
    public void Setup()
    {
        if (modConfig.BattleConfig.SimulationInterval <= 0)
            return;

        TimeSpan interval = TimeSpan.FromMinutes(modConfig.BattleConfig.SimulationInterval);
        _battleTimer = new Timer(_ =>
        {
            Simulate();
        }, null, interval, interval);
    }
    
    /*
     * Operate in stages:
     * 1. Spread to x nearby "none" locations next to current positions
     * 2. Start contesting x nearby enemy locations if currently uncontested
     * 3. Run battle calculations if there's a battle
     * 4. Build up strength if uncontested
     * 5. Roll for uprising faction presence
     * Run these 3 stages per location
     * Uncap these if configured to
     * Run the configured number of locations (x) per simulation
     */
    public void Simulate(string? raidLocation = null, Dictionary<string, int>? raidKills = null)
    {
        if (!modConfig.BattleConfig.BattlesEnabled)
            return;
        
        if (modConfig.Debug)
        {
            logger.Info("[TT] Starting simulation.");
        }

        RaidBattleState currentState = raidLocation == null ? RaidBattleState.Complete : RaidBattleState.Waiting;
        for (int i = 0; i < modConfig.BattleConfig.SimulationLocations; i++)
        {
            /*
             * If there was a raid:
             * 1. Run that raid first
             * 2. Block the first instance of that location if it's rotated to
             * 3. Run like normally
             */
            string currentLocation;
            if (currentState == RaidBattleState.Waiting && raidLocation != null)
            {
                currentLocation = raidLocation;
                stateServer.CurrentSave.LastLoc--; //don't skip the current location
                currentState = RaidBattleState.Blocking;
            }
            else
            {
                stateServer.CurrentSave.LastLoc = TerritoryMath.Wrap(stateServer.CurrentSave.LastLoc + 1, 0,
                    LocationService.MapList.Count);
                currentLocation = LocationService.MapList[stateServer.CurrentSave.LastLoc];
                
                if (currentState == RaidBattleState.Blocking && currentLocation == raidLocation)
                {
                    i--;
                    currentState = RaidBattleState.Complete;
                    continue;
                }
            }

            //don't simulate twice for these locations
            if (LocationService.DuplicateMapList.Contains(currentLocation))
            {
                i--;
                continue;
            }
            
            if (modConfig.Debug)
            {
                logger.Info($"[TT] Simulating location: {currentLocation}");
            }
            
            LocationState locState = stateServer.CurrentSave.Locations[currentLocation]!;
            
            if (locState.Holder == "none")
            {
                i--;
                continue;
            }
            
            SpreadNearby(currentLocation, locState);

            if (locState.Contestants.Count == 1)
            {
                SpreadNearby(currentLocation, locState, false);
            }
            
            CalculateBattle(currentLocation, locState, raidLocation == currentLocation ? raidKills : null);

            if (locState.Contestants.Count == 1 || locState.Base)
            {
                string holder = locState.Holder;
                double startingStrength = locState.Contestants[holder];

                if (startingStrength > modConfig.BattleConfig.MaxStrengthBuildup)
                    continue;

                locState.Contestants[holder] = Math.Clamp(startingStrength + modConfig.BattleConfig.StrengthBuildup, 0, 
                    Math.Min(1, modConfig.BattleConfig.MaxStrengthBuildup));

                if (modConfig.Debug)
                {
                    logger.Info($"[TT] Faction {holder} has gone from {startingStrength} to {locState.Contestants[holder]} strength at {currentLocation}.");
                }
            }

            if (modConfig.BattleConfig.Uprising)
            {
                CheckUprisings(locState, currentLocation);
            }
        }
        
        locationService.UpdateLocations();
        stateServer.SaveToDisk();
    }

    private void SpreadNearby(string location, LocationState locState, bool noneOnly = true)
    {
        string faction = locState.Holder;
        Faction factionData = dataConfig.Factions[faction];
        BattleConfig battleConfig = modConfig.BattleConfig;
        
        double distanceReduction = battleConfig.StrengthDecrease > 0
            ? battleConfig.StrengthDecrease
            : factionData.DistanceReduction;

        double factionStrength = locState.Contestants[faction];
        if (factionStrength < battleConfig.SpreadMinStrength)
            return;

        double moveStrength = factionStrength - distanceReduction;

        if (moveStrength <= 0.1) return;
        
        List<string> nearby = FindNearby(location, noneOnly ? "none" : null);
        double moveCost = moveStrength * battleConfig.SpreadMult;

        //calculate how many actions can be taken before going under the spread threshold
        int maxActions = 0;
        while (factionStrength > battleConfig.SpreadMinStrength)
        {
            factionStrength -= moveCost;
            maxActions++;
        }

        maxActions = Math.Min(maxActions, battleConfig.SimulationActions);

        while (nearby.Count > maxActions)
        {
            nearby.RemoveAt(randomUtil.GetInt(0, nearby.Count - 1));
        }

        //a little above 0, don't want to add killing implementation.
        locState.Contestants[faction] = Math.Clamp(locState.Contestants[faction] - nearby.Count * moveCost, 0.01, 1);
        if (modConfig.Debug && nearby.Count > 0)
        {
            logger.Info($"[TT] Faction {faction} is spreading from {location} with a cost of {nearby.Count * moveCost}.");
        }

        moveStrength = Math.Clamp(moveStrength + battleConfig.SpreadBonus, 0, 1);
        foreach (string neighbor in nearby)
        {
            if (modConfig.Debug)
            {
                logger.Info($"[TT] Faction {faction} is spreading from location: {location} to: {neighbor}.");
            }
            
            LocationState newState = stateServer.CurrentSave.Locations[neighbor]!;

            if (noneOnly)
                newState.Holder = faction;
            
            newState.Contestants[faction] = moveStrength;
        }
    }

    //the goal is to calculate a battle between n contestants each with a strength 0.0 - 1.0 and defensiveness 0.0 - 1.0
    //each contestant will attack each other if not allies
    //spread out damage over all enemies
    //holder gets a strength buff
    //reduce all incoming damage once by the defensiveness
    //use percentage of location strength and faction strength to reduce values
    private void CalculateBattle(string locationName, LocationState locationState, Dictionary<string, int>? kills = null)
    {
        if (locationState.Contestants.Count < 2)
            return;

        Dictionary<string, double> damageDealt = [];

        //raid kills
        if (kills != null)
        {
            foreach ((string botName, int deaths) in kills)
            {
                string factionName = dataConfig.BotFaction.GetValueOrDefault(botName, "none");
                
                if (!locationState.Contestants.ContainsKey(factionName))
                    continue;

                Faction faction = dataConfig.Factions[factionName];
                bool boss = faction.MobileBossNames.Contains(botName) || faction.BossNames.Contains(botName);
                
                double enemyDamage = boss ? modConfig.BattleConfig.RaidBossStrengthLoss :
                    modConfig.BattleConfig.RaidStrengthLoss;

                double damage = deaths * enemyDamage;

                if (!damageDealt.TryAdd(factionName, damage))
                    damageDealt[factionName] += damage;
            }
        }

        //save
        int contestants = locationState.Contestants.Count;
        
        //simulation damage
        foreach ((string contestant, double strength) in locationState.Contestants)
        {
            Faction factionData = dataConfig.Factions[contestant];

            List<string> targets = [];

            foreach (string other in locationState.Contestants.Keys)
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
            if (contestant == locationState.Holder)
            {
                updatedStrength += factionData.Defensiveness * locationState.Contestants[locationState.Holder];
            }

            double damage = (updatedStrength / targets.Count) * modConfig.BattleConfig.DamageMultiplier;

            damage += randomUtil.RandNum(modConfig.BattleConfig.DamageMinRng, modConfig.BattleConfig.DamageMaxRng);

            foreach (string target in targets)
            {
                if (!damageDealt.TryAdd(target, damage))
                    damageDealt[target] += damage;
            }
        }

        foreach ((string faction, double damageTaken) in damageDealt)
        {
            if (!locationState.Contestants.ContainsKey(faction))
            {
                logger.Warning($"[TT] Battle at {locationName} had damage dealt to {faction} which isn't present.");
                continue;
            }
            
            if (!modConfig.BattleConfig.BaseTakingEnabled 
                && faction == locationState.Holder && locationState.Base)
                continue;
            
            Faction factionData = dataConfig.Factions[faction];

            double defenseDecrease = (factionData.Defensiveness * locationState.Contestants[faction]) 
                                     / contestants;
            double finalDamage = Math.Clamp(damageTaken - defenseDecrease, 0.0, 1.0);

            locationState.Contestants[faction] -= finalDamage;

            if (modConfig.Debug)
            {
                logger.Info($"[TT] Contestant {faction} at location: {locationName} has taken {finalDamage} and now has {locationState.Contestants[faction]} strength left.");
            }
            
            if (!(locationState.Contestants[faction] <= 0)) continue;
            
            if (modConfig.Debug)
            {
                logger.Info($"[TT] Contestant {faction} at location: {locationName} has been removed.");
            }
            
            locationState.Contestants.Remove(faction);
            
            if (faction == locationState.Holder)
                locationState.Base = false;
            
            foreach (string neighbor in dataConfig.LocationNeighbors[locationName]!)
            {
                RemoveIsolatedContestant(neighbor, faction);
            }
        }

        //figure out holder swapping
        
        if (locationState.Contestants.Count == 0)
        {
            locationState.Holder = "none";
            return;
        }
        
        if (locationState.Contestants.ContainsKey(locationState.Holder))
            return;

        double highestStrength = 0.0;
        string strongestFaction = "none";

        foreach ((string contestant, double strength) in locationState.Contestants)
        {
            if (!(strength > highestStrength)) continue;
            
            highestStrength = strength;
            strongestFaction = contestant;
        }

        locationState.Holder = strongestFaction;
    }

    //null target gets all nearby not of the same faction
    private List<string> FindNearby(string start, string? targetFaction)
    {
        List<string> results = [];

        LocationState currentLoc = stateServer.CurrentSave.Locations[start]!;
        
        List<string> neighbors = dataConfig.LocationNeighbors[start]!;
        foreach (string neighborLocation in neighbors)
        {
            string otherHolder = stateServer.CurrentSave.Locations[neighborLocation]!.Holder;

            if ((targetFaction == null && currentLoc.Holder != otherHolder) ||
                otherHolder == targetFaction)
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
        
        LocationState locState = stateServer.CurrentSave.Locations[location]!;

        if (locState.Holder == faction || !locState.Contestants.ContainsKey(faction))
            return;

        bool isolated = false;
        foreach (string neighbor in dataConfig.LocationNeighbors[location]!)
        {
            LocationState neighborState = stateServer.CurrentSave.Locations[neighbor]!;

            if (neighborState.Holder != faction) continue;
            
            isolated = true;
            break;
        }

        if (isolated)
        {
            locState.Contestants.Remove(faction);
        }
    }

    private void CheckUprisings(LocationState locState, string locationName)
    {
        List<string> uprisingPool = [];
        foreach ((string factionName, Faction faction) in dataConfig.Factions)
        {
            if (factionName == "none" 
                || faction.UprisingChance <= 0 
                || locState.Contestants.ContainsKey(factionName))
                continue;
                
            //only roll if the previous conditions are false
            if (!randomUtil.GetChance100(faction.UprisingChance)) continue;

            uprisingPool.Add(factionName);
        }

        while (uprisingPool.Count >= modConfig.BattleConfig.SimulationActions)
            uprisingPool.RemoveAt(randomUtil.GetInt(0, uprisingPool.Count - 1));

        foreach (string factionName in uprisingPool)
        {
            locState.Contestants[factionName] =
                Math.Clamp(dataConfig.Factions[factionName].Strength * modConfig.BattleConfig.UprisingMult, 0, 1);

            if (locState.Holder == "none")
                locState.Holder = factionName;

            if (modConfig.Debug)
            {
                logger.Info($"[TT] Faction {factionName} has started an uprising at {locationName} with {locState.Contestants[factionName]} strength.");
            }
        }
    }
}