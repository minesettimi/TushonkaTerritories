using System;
using System.Collections.Generic;
using Diz.Utils;
using EFT;
using JsonType;
using TerritoryClient.Extensions;
using TerritoryClient.Models;
using Unity.Mathematics;
using UnityEngine;

namespace TerritoryClient.Spawns;

//faction conscious non-wave based bot spawner
public class TerritoriesSpawnScenario : MonoBehaviour
{
    private AbstractGame _game = null!;
    private LocationSettings.Location _location = null!;
    private BotsController? _botsController;
    private LocationState _locationState = null!;
    private Dictionary<string, FactionData> _factions = null!;
    private SpawnSettings _spawnSettings = null!;
    private LocationSpawnSettings _locationSettings = null!;

    private bool _enabled;
    private bool _started;
    private bool _spawnActive;
    private bool _atMaxCap;
    
    private float _nextWindow = -1;
    private float _nextCheck = -1;
    private int _currentContestant = 0;

    public CountTypeBotWave[] BotData { get; set; } = [];

    private readonly Dictionary<double, WDictionary<BotDifficulty>> _difficultyWeights = [];
    private readonly Dictionary<string, ContestantSpawnInfo> _factionSpawnData = [];
    private readonly List<string> _factionIndex = [];
    
    private int _endTime;
    private float _totalStrength;
    
    #region Initalizing
    private void Init()
    {
        LocationState? tempState = TerritoryPlugin.StateManager.State.Locations[_location.Id];
        if (tempState == null)
            return;

        if (TerritoryPlugin._debug.Value)
        {
            TerritoryPlugin.PluginLogger.LogInfo("Initializing spawn scenario!");
        }

        _enabled = true;
        _locationState = tempState;
        _spawnSettings = TerritoryPlugin.StateManager.ServerData.SpawnSettings;
        _factions = TerritoryPlugin.StateManager.ServerData.Factions;
        _locationSettings = _spawnSettings.MapSettings[_location.Id.ToLower()]!;

        _endTime = (_location.EscapeTimeLimit * 60) - _spawnSettings.EndTime;
        
        InitializeWeights();
        InitializeBotTypes();
        InitializeBotCaps();
        SetupBots();
    }

    private void InitializeWeights()
    {
        if (_spawnSettings.DifficultyWeights == null)
        {
            _difficultyWeights[1.0] = new WDictionary<BotDifficulty>
            {
                { _location.BotEasy, BotDifficulty.easy },
                { _location.BotNormal, BotDifficulty.normal },
                { _location.BotHard, BotDifficulty.hard },
                { _location.BotImpossible, BotDifficulty.impossible }
            };
            return;
        }
        
        foreach ((double diffThreshold, Dictionary<BotDifficulty, int> difficultyWeights) 
                 in _spawnSettings.DifficultyWeights)
        {
            WDictionary<BotDifficulty> weightDict = new();
            foreach ((BotDifficulty difficulty, int weight) in difficultyWeights)
            {
                weightDict.Add(weight, difficulty);
            }
            
            _difficultyWeights[diffThreshold] = weightDict;
        }
    }

    private void InitializeBotTypes()
    {
        foreach ((string factionName, double strength) in _locationState.Contestants)
        {
            if (factionName == "none")
                continue;
            
            float flStrength =　(float)strength;
            _totalStrength += flStrength;
            _factionIndex.Add(factionName);

            int minGroupSize = Mathf.RoundToInt(math.remap(0f, 1f, _spawnSettings.GroupSizeMin.Min,
                _spawnSettings.GroupSizeMax.Max, flStrength));
            int maxGroupSize = Mathf.RoundToInt(math.remap(0f, 1f, _spawnSettings.GroupSizeMax.Min,
                _spawnSettings.GroupSizeMax.Max, flStrength));
            int groupChance = Mathf.RoundToInt(math.remap(0f, 1f, _spawnSettings.GroupChance.Min,
                _spawnSettings.GroupChance.Max, flStrength));

            ContestantSpawnInfo contestantInfo = new(minGroupSize, maxGroupSize, flStrength, groupChance);
            foreach ((double threshold, WDictionary<BotDifficulty> weightDict) in _difficultyWeights)
            {
                if (flStrength > threshold)
                    continue;
                
                contestantInfo.DifficultyWeights = weightDict;
                break;
            }

            _factionSpawnData[factionName] = contestantInfo;
        }
    }

    private void InitializeBotCaps()
    {
        int maxBots = _locationSettings.MaxBots - _spawnSettings.SoftCapSpace;

        foreach ((string factionName, ContestantSpawnInfo contestantInfo) in _factionSpawnData)
        {
            float partition = contestantInfo.Strength / _totalStrength;
            contestantInfo.MaxCap = Mathf.RoundToInt(maxBots * partition);

            if (TerritoryPlugin._debug.Value)
            {
                TerritoryPlugin.PluginLogger.LogInfo(
                    $"Faction: {factionName} has partition: {partition} of maxbots: {maxBots}, maxcount: {_botsController!._maxCount}");
            }
        }
    }

    private void SetupBots()
    {
        List<WildSpawnType> allBots = [];
        List<CountTypeBotWave> botWaves = [];

        foreach ((string factionName, ContestantSpawnInfo spawnInfo) in _factionSpawnData)
        {
            FactionData factionData = _factions[factionName];
            allBots.AddRange(factionData.BotTypes);
            
            for (int i = 0; i < spawnInfo.MaxCap; i++)
            {
                botWaves.Add(new CountTypeBotWave(_spawnSettings.PerBotLoadout, factionData.BotTypes.PickRandom(),
                    _factionSpawnData[factionName].DifficultyWeights.Random()));
            }
        }

        foreach (WildSpawnType bot in allBots)
        {
            botWaves.Add(new CountTypeBotWave(_spawnSettings.ExtraLoadouts, bot, BotDifficulty.normal));
        }

        BotData = [.. botWaves];
    }

    public void Run()
    {
        if (_enabled && _botsController != null)
        {
            if (TerritoryPlugin._debug.Value)
            {
                TerritoryPlugin.PluginLogger.LogInfo("Starting spawn scenario!");
            }
            
            _currentContestant = 0;
            _started = true;
        }
    }

    public void Stop()
    {
        if (TerritoryPlugin._debug.Value)
        {
            TerritoryPlugin.PluginLogger.LogInfo("Stopping spawn scenario!");
        }

        _started = false;
    }
    #endregion

    #region Spawning

    private void Update()
    {
        if (!_started)
            return;

        if (_game.PastTime < _spawnSettings.StartTime || _game.PastTime > _endTime)
            return;

        if (_nextWindow <= _game.PastTime)
        {
            _spawnActive = !_spawnActive;
            _nextWindow = _game.PastTime + (_spawnActive ? 
                MyExtensions.Random(_locationSettings.SpawnOnWindow.Min, _locationSettings.SpawnOnWindow.Max) : 
                MyExtensions.Random(_locationSettings.SpawnOffWindow.Min, _locationSettings.SpawnOffWindow.Max));
        }
        //revive dead raids by fast tracking off times
        else if (!_spawnActive 
                 && _botsController!.AliveAndLoadingBotsCount <= _spawnSettings.DeadRaidBots 
                 && _nextWindow - _game.PastTime > _spawnSettings.DeadRaidTime)
        {
            _nextWindow = _game.PastTime + _spawnSettings.DeadRaidTime;
        }

        if (!_spawnActive)
            return;

        if (_nextCheck > _game.PastTime)
            return;

        _nextCheck = _game.PastTime + Math.Max(_spawnSettings.SpawnCheck, 15f); //specific number

        int botCountRemaining = _locationSettings.MaxBots - _botsController!.AliveLoadingDelayedBotsCount - 
                                _spawnSettings.SoftCapSpace;
        if (_atMaxCap)
        {
            if (botCountRemaining < _spawnSettings.RequiredSpawnSpace)
                return;

            _atMaxCap = false;
        }
        else if (botCountRemaining <= 0)
        {
            _atMaxCap = _locationSettings.MaxBots - _botsController.AliveAndLoadingBotsCount -
                _spawnSettings.SoftCapSpace <= 0;
            return;
        }

        string spawnFaction = _factionIndex[_currentContestant];
        _currentContestant = (_currentContestant + 1) % _factionIndex.Count;

        if (TerritoryPlugin._debug.Value)
        {
            TerritoryPlugin.PluginLogger.LogInfo($"Faction spawning: {spawnFaction}");
        }

        ContestantSpawnInfo spawnInfo = _factionSpawnData[spawnFaction];
        FactionData factionData = _factions[spawnFaction];
        
        int currentFactionBots = _botsController!.Bots.GetBotCountByFaction(spawnFaction); //probably expensive, oh well!

        if (TerritoryPlugin._debug.Value)
        {
            TerritoryPlugin.PluginLogger.LogInfo($"Current bots: {currentFactionBots}");
        }
        int factionBotsRemaining = Math.Min(Math.Min(botCountRemaining, spawnInfo.MaxCap - currentFactionBots), _spawnSettings.MaxIntervalSpawns);

        if (TerritoryPlugin._debug.Value)
        {
            TerritoryPlugin.PluginLogger.LogInfo(
                $"Faction bot slots: {factionBotsRemaining} out of {spawnInfo.MaxCap}.");
        }

        if (factionBotsRemaining <= 0)
            return;

        if (factionBotsRemaining >= spawnInfo.MinGroupSize)
        {
            if (SpawnBotGroups(ref factionBotsRemaining, spawnInfo, factionData) && _spawnSettings.GroupSpawnExclusive)
                return;
        }

        int soloBots = Mathf.CeilToInt(factionBotsRemaining * _spawnSettings.NonGroupBotModifier);
        
        for (int i = 0; i < soloBots; i++)
            SpawnByWave(1, factionData, spawnInfo);
    }

    private bool SpawnBotGroups(ref int availableBots, ContestantSpawnInfo spawnInfo, FactionData factionData)
    {
        int groupChances = Mathf.Max(Mathf.FloorToInt(availableBots / (float)spawnInfo.MaxGroupSize), 1);

        for (int i = 0; i < groupChances; i++)
        {
            if (!MyExtensions.IsTrue100(spawnInfo.GroupChance)) continue;
            
            int groupSize = MyExtensions.RandomInclude(spawnInfo.MinGroupSize,
                Mathf.Min(spawnInfo.MaxGroupSize, availableBots));
            
            availableBots -= groupSize;
            SpawnByWave(groupSize, factionData, spawnInfo);

            if (TerritoryPlugin._debug.Value)
            {
                TerritoryPlugin.PluginLogger.LogInfo($"Spawning bots by group size: {groupSize}.");
            }

            return true;
        }

        return false;
    }

    private void SpawnByWave(int count, FactionData factionData, ContestantSpawnInfo spawnInfo)
    {
        WildSpawnType botType = factionData.BotTypes.Random();
        BotDifficulty botDifficulty = spawnInfo.DifficultyWeights.Random();
        EPlayerSide playerSide = botType switch
        {
            WildSpawnType.pmcUSEC => EPlayerSide.Usec,
            WildSpawnType.pmcBEAR => EPlayerSide.Bear,
            _ => EPlayerSide.Savage
        };

        bool isSniper = botType == WildSpawnType.marksman;
        BotZone? spawnZone = _botsController!._botSpawner.GetRandomBotZoneExclusive(isSniper);
        
        //no sniping zones, don't spawn
        if (spawnZone is null)
        {
            return; //don't want to swap to a different bot, otherwise it will hardcode a bot
        }
        
        _botsController!.ActivateBotsByWave(new SpawnWave
        {
            Time = Time.time + MyExtensions.Random(0, _spawnSettings.SpawnDelayAdjustmentMax),
            Difficulty = botDifficulty,
            BotsCount = count,
            Side = playerSide,
            WildSpawnType = botType,
            ChanceGroup = 0,
            SpawnAreaName = spawnZone.name,
            IsPlayers = false,
            WithCheckMinMax = false
        });
    }

    #endregion
    
    public static TerritoriesSpawnScenario Create(AbstractGame game, LocationSettings.Location location,
        BotsController botsController)
    {
        TerritoriesSpawnScenario spawnScenario = game.gameObject.AddComponent<TerritoriesSpawnScenario>();
        spawnScenario._game = game;
        spawnScenario._location = location;
        spawnScenario._botsController = botsController;
        spawnScenario.Init();

        return spawnScenario;
    }
}

public class ContestantSpawnInfo(int minGroupSize, int maxGroupSize, float strength, int groupChance)
{
    public int MinGroupSize { get; } = minGroupSize;
    public int MaxGroupSize { get; } = maxGroupSize;
    public int GroupChance { get; } = groupChance;
    public float Strength { get; } = strength;

    public int MaxCap { get; set; }
    public WDictionary<BotDifficulty> DifficultyWeights = [];
}