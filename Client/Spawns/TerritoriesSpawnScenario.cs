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
    private BotsController? _botsController = null!;
    private LocationState _locationState = null!;
    private Dictionary<string, FactionData> _factions = null!;
    private SpawnSettings _spawnSettings = null!;

    private bool _enabled;
    private bool _started;
    private bool _spawnActive;
    private bool _atMaxCap;
    
    private float _nextWindow = -1;
    private float _nextCheck = -1;
    private string? _lastContestant;

    public CountTypeBotWave[] BotData { get; set; } = [];

    private readonly Dictionary<double, WDictionary<BotDifficulty>> _difficultyWeights = [];
    private readonly Dictionary<string, ContestantSpawnInfo> _factionSpawnData = [];
    private readonly WDictionary<string> _factionWeights = new();
    
    #region Initalizing
    private void Init()
    {
        LocationState? tempState = TerritoryPlugin.StateManager.State.Locations[_location.Id];
        if (tempState == null)
            return;

        _enabled = true;
        _locationState = tempState;
        _spawnSettings = TerritoryPlugin.StateManager.ServerData.SpawnSettings;
        _factions = TerritoryPlugin.StateManager.ServerData.Factions;
        
        InitializeWeights();
        InitializeBotTypes();
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
        float totalStrength = 0;
        
        foreach ((string factionName, double strength) in _locationState.Contestants)
        {
            if (factionName == "none")
                continue;
            
            float flStrength =　(float)strength;
            totalStrength += flStrength;
            
            _factionWeights.Add(flStrength, factionName);

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
                
                _factionSpawnData[factionName].DifficultyWeights = weightDict;
                break;
            }

            _factionSpawnData[factionName] = contestantInfo;
        }

        int maxBots = _botsController!._maxCount / (_locationState.Contestants.Count - _spawnSettings.SoftCapSpace);

        foreach ((string factionName, ContestantSpawnInfo contestantInfo) in _factionSpawnData)
        {
            float partition = contestantInfo.Strength / totalStrength;
            contestantInfo.MaxCap = Mathf.RoundToInt(maxBots * partition);
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
            _lastContestant = null;
            _started = true;
        }
    }

    public void Stop()
    {
        _started = false;
    }
    #endregion

    #region Spawning

    private void Update()
    {
        if (!_started)
            return;

        if (_game.PastTime < _spawnSettings.StartTime || _game.PastTime > _spawnSettings.EndTime)
            return;

        if (_nextWindow <= _game.PastTime)
        {
            _spawnActive = !_spawnActive;
            _nextWindow = _game.PastTime + (_spawnActive ? 
                MyExtensions.Random(_spawnSettings.SpawnOnWindow.Min, _spawnSettings.SpawnOnWindow.Max) : 
                MyExtensions.Random(_spawnSettings.SpawnOffWindow.Min, _spawnSettings.SpawnOffWindow.Max));
        }

        if (!_spawnActive)
            return;

        if (_nextCheck > _game.PastTime)
            return;

        _nextCheck = _game.PastTime + Math.Max(_spawnSettings.SpawnCheck, 15f); //specific number

        int botCountRemaining = _botsController!._maxCount - _botsController!.AliveAndLoadingBotsCount - 
                                _spawnSettings.SoftCapSpace;

        if (_atMaxCap)
        {
            if (botCountRemaining < _spawnSettings.RequiredSpawnSpace)
                return;

            _atMaxCap = false;
        }
        else if (botCountRemaining <= 0)
        {
            _atMaxCap = _botsController!._maxCount - _botsController.AliveAndLoadingBotsCount -
                _spawnSettings.SoftCapSpace <= 0;
            return;
        }

        string spawnFaction;
        do
        {
            spawnFaction = _factionWeights.Random();
        } while (_factions.Count > 1 && spawnFaction == _lastContestant);
        
        ContestantSpawnInfo spawnInfo = _factionSpawnData[spawnFaction];
        FactionData factionData = _factions[spawnFaction];
        
        int currentFactionBots = _botsController!.Bots.GetBotCountByFaction(spawnFaction); //probably expensive, oh well!
        int factionBotsRemaining = Math.Min(botCountRemaining, spawnInfo.MaxCap - currentFactionBots);

        if (factionBotsRemaining >= spawnInfo.MinGroupSize)
        {
            SpawnBotGroups(ref factionBotsRemaining, spawnInfo, factionData);
        }

        for (int i = 0; i < factionBotsRemaining; i++)
        {
            WildSpawnType botType = factionData.BotTypes.Random();
            BotDifficulty botDifficulty = spawnInfo.DifficultyWeights.Random();
            EPlayerSide playerSide = botType switch
            {
                WildSpawnType.pmcUSEC => EPlayerSide.Usec,
                WildSpawnType.pmcBEAR => EPlayerSide.Bear,
                _ => EPlayerSide.Savage
            };

            //TODO: maybe adjustable spawn time
            GetProfileDataParams profileDataParams = new(playerSide, botType, botDifficulty, 0f);
            _botsController!.ActivateBotsWithoutWave(1, profileDataParams);
        }
    }

    private void SpawnBotGroups(ref int availableBots, ContestantSpawnInfo spawnInfo, FactionData factionData)
    {
        int groupCount = Mathf.Max(Mathf.RoundToInt(availableBots / (float)spawnInfo.MaxGroupSize), 1);

        for (int i = 0; i < groupCount; i++)
        {
            if (MyExtensions.IsTrue100(spawnInfo.GroupChance))
            {
                int groupSize = MyExtensions.RandomInclude(spawnInfo.MinGroupSize,
                    Mathf.Min(spawnInfo.MaxGroupSize, availableBots));
                availableBots -= groupSize;
                
                SpawnGroup(groupSize, spawnInfo, factionData);
                //break; TODO: test this
            }
        }
    }

    private void SpawnGroup(int size, ContestantSpawnInfo spawnInfo,　FactionData factionData)
    {
        WildSpawnType botType = factionData.BotTypes.Random();
        BotDifficulty botDifficulty = spawnInfo.DifficultyWeights.Random();
        EPlayerSide playerSide = botType switch
        {
            WildSpawnType.pmcUSEC => EPlayerSide.Usec,
            WildSpawnType.pmcBEAR => EPlayerSide.Bear,
            _ => EPlayerSide.Savage
        };
        
        GetProfileDataParams profileDataParams = new(playerSide, botType, botDifficulty, 0f, new BotSpawnParams())
        {
            _spawnParams =
            {
                ShallBeGroup = new ShallBeGroupParams(true, true, size)
            }
        };
        
        _botsController!.ActivateBotsWithoutWave(size, profileDataParams);
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