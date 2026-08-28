using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using TerritoryServer.Models;
using TerritoryServer.Servers;
using TerritoryServer.Utils;

namespace TerritoryServer.Services;

[Injectable(InjectionType.Singleton)]
public class LocationService(DataConfig dataConfig,
    LocationTable locationTable,
    ModConfig modConfig,
    StateServer stateServer,
    BotConfig botConfig,
    MathUtil mathUtil,
    RandomUtil randomUtil,
    ISptLogger<LocationService> logger,
    ICloner cloner)
{
    public static readonly List<string> MapList = 
    [
        "bigmap",
        "factory4_day",
        "factory4_night",
        "interchange",
        "laboratory",
        "lighthouse",
        "rezervbase",
        "sandbox",
        "sandbox_high",
        "shoreline",
        "tarkovstreets",
        "woods",
        "labyrinth",
        "suburbs",
        "terminal"
    ];

    public static readonly List<string> DuplicateMapList =
    [
        "factory4_night",
        "sandbox_high"
    ];

    private static readonly List<string> Difficulties =
    [
        "impossible",
        "hard",
        "normal",
        "easy"
    ];

    enum BotRelationship
    {
        Friends,
        Neutral,
        Enemies
    }

    private Dictionary<string, BossLocationSpawn> _mobileBossData = [];
    
    private Dictionary<string, List<BossLocationSpawn>> _bossBackup = [];
    private Dictionary<string, List<Wave>> _waveBackup = [];
    private Dictionary<string, IEnumerable<AdditionalHostilitySettings>> _hostilityBackup = [];
    private IEnumerable<AdditionalHostilitySettings> _hostilityCache = [];

    public void Initialize()
    {
        BackupLocationData();
        AdjustLocationSettings();
        BuildHostilityCache();
        UpdateLocations();
        
        logger.Info("[TT] Finished initializing location data.");
    }
    
    private void BackupLocationData()
    {
        List<string> bossesToRemove = [];

        foreach (Faction faction in dataConfig.Factions.Values)
        {
            bossesToRemove.AddRange(faction.MobileBossNames);
        }

        foreach (string locationName in MapList)
        {
            Location locationInfo = locationTable.GetLocation(locationName)!;
            
            _bossBackup.Add(locationName, []);
            foreach (BossLocationSpawn bossSpawn in locationInfo.Base.BossLocationSpawn)
            {
                BossLocationSpawn clonedSpawn = cloner.Clone(bossSpawn)!;
                if (!_mobileBossData.ContainsKey(clonedSpawn.BossName!) 
                    && bossesToRemove.Contains(bossSpawn.BossName!))
                {
                    clonedSpawn.TriggerId = "";
                    clonedSpawn.TriggerName = "";
                    clonedSpawn.BossZone = "";
                    clonedSpawn.ShowOnTarkovMap = false;
                    clonedSpawn.ShowOnTarkovMapPvE = false;
                    
                    _mobileBossData.Add(clonedSpawn.BossName!, clonedSpawn);
                    continue;
                }
                
                _bossBackup[locationName].Add(clonedSpawn);
            }
            
            _waveBackup.Add(locationName, cloner.Clone(locationInfo.Base.Waves)!);
            _hostilityBackup.Add(locationName,
                cloner.Clone(locationInfo.Base.BotLocationModifier.AdditionalHostilitySettings)!);
        }
    }

    //credit to acidphantasm for originally finding the data that needed to be changed
    private void AdjustLocationSettings()
    {
        foreach (string locationName in MapList)
        {
            LocationBase location = locationTable.GetLocation(locationName)!.Base;
            
            location.Waves = [];
            location.NewSpawn = false;
            location.OfflineNewSpawn = false;
            location.OldSpawn = true;
            location.OfflineOldSpawn = true;

            if (!botConfig.PlayerScavBrainType.ContainsKey(locationName))
            {
                botConfig.PlayerScavBrainType.Add(locationName, cloner.Clone(botConfig.PlayerScavBrainType["tarkovstreets"])!);
            }

            if (!botConfig.AssaultBrainType.ContainsKey(locationName))
            {
                botConfig.AssaultBrainType.Add(locationName, cloner.Clone(botConfig.AssaultBrainType["tarkovstreets"])!);
            }
        }
    }
    
    //null updates all valid locations
    public void UpdateLocations(List<string>? maps = null)
    {
        RaidConfig raidConfig = modConfig.RaidConfig;
        foreach (string locationName in maps ?? MapList)
        {
            LocationBase location = locationTable.GetLocation(locationName)!.Base;
            LocationState locationState = stateServer.CurrentSave.Locations[locationName]!;

            //re-uses previous configuration if there's no faction, change if not desired
            if (locationState.Holder == "none")
                continue;
            
            List<BossLocationSpawn> newSpawns = cloner.Clone(_bossBackup[locationName])!;
            for (int i = 0; i < newSpawns.Count; i++)
            {
                BossLocationSpawn bossSpawn = newSpawns[i];

                if (!raidConfig.OverrideTriggeredSpawns &&
                    (bossSpawn.TriggerId?.Length > 2 || bossSpawn.TriggerName?.Length > 2))
                    continue;
                        
                bool isPmc = bossSpawn.BossName == "pmcBEAR" || bossSpawn.BossName == "pmcUSEC";
                bool isCultist = bossSpawn.BossName.StartsWith("sectant");
                
                if ((raidConfig.OverridePmcs && isPmc) || (raidConfig.OverrideCultists && isCultist) || (raidConfig.OverrideBosses && !isPmc && !isCultist))
                {
                    newSpawns.RemoveAt(i);
                    i--;
                }
                else if (isPmc && !raidConfig.OverridePmcs && raidConfig.EnforcePmcSpawns)
                {
                    newSpawns[i].ForceSpawn = true;
                }
            }
            
            newSpawns.AddRange(BuildCustomSpawns(locationState, location.BotMax, (double)location.EscapeTimeLimit!));
            
            location.BossLocationSpawn = randomUtil.Shuffle(newSpawns);
            
            if (raidConfig.AttitudeEffect)
            {
                location.BotLocationModifier.AdditionalHostilitySettings = cloner.Clone(_hostilityCache)!;
            }

            if (raidConfig.OverrideWaves)
            {
                location.Waves.Clear();
            }
            else
                location.Waves = cloner.Clone(_waveBackup[locationName])!;
        }
        
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
    }

    //first, setup base spawns
    //second, setup wave
    private List<BossLocationSpawn> BuildCustomSpawns(LocationState locationState, int maxBots, double timeLimit)
    {
        List<BossLocationSpawn> newSpawns = [];
        RaidConfig raidConfig = modConfig.RaidConfig;

        double trueTimeLimit = (timeLimit * 60) - raidConfig.SpawnEnd;

        //make equal space per faction
        int maxBotsPerWave = maxBots / locationState.Contestants.Count;
        
        foreach ((string factionName, double strength) in locationState.Contestants)
        {
            Faction currentFaction = dataConfig.Factions[factionName];
            if (raidConfig.FactionBosses && strength >= raidConfig.MinBossStrength)
            {
                int bossChance = Math.Clamp((int)Math.Round(mathUtil.MapToRange(strength, 0.0,
                    1.0, raidConfig.MinBossChance, raidConfig.MaxBossChance)), 0, 100);
                
                foreach (string bossName in currentFaction.MobileBossNames)
                {
                    BossLocationSpawn clonedSpawn = cloner.Clone(_mobileBossData[bossName])!;
                    clonedSpawn.BossChance = bossChance;
                    
                    newSpawns.Add(clonedSpawn);
                }
            }
            
            int spawnDelay = (int)Math.Round(mathUtil.MapToRange(1 - strength, 0, 1,
                raidConfig.MinWaveDelay, raidConfig.MaxWaveDelay));

            int waves = (int)Math.Floor(trueTimeLimit / randomUtil.GetInt(spawnDelay - raidConfig.DelayVariance,
                spawnDelay + raidConfig.DelayVariance));
            int baseBotCount = (int)Math.Round(mathUtil.MapToRange(strength, 0, 1,
                raidConfig.MinWaveBotCount, raidConfig.MaxWaveBotCount));

            if (modConfig.RaidConfig.MaxWaveCap >= 0)
                waves = Math.Min(modConfig.RaidConfig.MaxWaveCap, waves);
            
            for (int i = 0; i <= waves; i++)
            {
                int remainingBots = baseBotCount;

                if (i == 0)
                {
                    remainingBots = (int)Math.Round(remainingBots * raidConfig.InitialBotMult);
                }

                remainingBots = Math.Min(remainingBots, (int)Math.Round(maxBotsPerWave * strength));
                
                int currentDelay = 1 + (i * spawnDelay);

                bool enforceWave = raidConfig.EnforceBotSpawns || (i == 0 && raidConfig.EnforceFirstWave);

                while (remainingBots > 0)
                {
                    int groupSize;

                    if (randomUtil.GetChance100(raidConfig.GroupChance))
                        groupSize = (int)Math.Round(mathUtil.MapToRange(strength, 0, 1,
                            raidConfig.MinStrengthUnits, raidConfig.MaxStrengthUnits)) + 
                                randomUtil.RandInt(-raidConfig.VariedGroupSize, raidConfig.VariedGroupSize + 1);
                    else
                        groupSize = 1;

                    if (!raidConfig.RoundedBotCounts)
                    {
                        groupSize = Math.Min(groupSize, remainingBots);
                    }

                    remainingBots -= groupSize;

                    BossLocationSpawn newBotSpawn = new()
                    {
                        BossChance = 100,
                        Delay = 0,
                        BossDifficulty = GetDifficultyFromStrength(strength),
                        BossEscortDifficulty = GetDifficultyFromStrength(strength),
                        BossName = randomUtil.GetRandomElement(currentFaction.BotNames),
                        BossEscortType = randomUtil.GetRandomElement(currentFaction.BotNames),
                        BossZone = "",
                        IsBossPlayer = false,
                        Time = currentDelay,
                        BossEscortAmount = groupSize == 1 ? "0" : groupSize.ToString(),
                        IgnoreMaxBots = false,
                        ForceSpawn = enforceWave,
                        IsRandomTimeSpawn = false,
                        SpawnMode = ["pve", "regular"],
                        TriggerId = "",
                        TriggerName = "",
                        ShowOnTarkovMap = false,
                        ShowOnTarkovMapPvE = false
                    };
                    
                    newSpawns.Add(newBotSpawn);

                    if (i > 0)
                        currentDelay += randomUtil.RandInt(0, 5);
                }
            }
        }

        return newSpawns;
    }

    private string GetDifficultyFromStrength(double strength)
    {
        foreach (string difficulty in Difficulties)
        {
            if (modConfig.RaidConfig.DifficultyThresholds[difficulty] <= strength && 
                !randomUtil.GetChance100(modConfig.RaidConfig.DifficultyChance))
            {
                return difficulty;
            }
        }
            
        return "normal";
    }
    
    private void BuildHostilityCache()
    {
        List<AdditionalHostilitySettings> finalSettings = [];
        
        foreach ((string botName, string botFaction) in dataConfig.BotFaction)
        {
            //player behavior is handled by a patch, don't mess with it
            AdditionalHostilitySettings newSettings = new()
            {
                AlwaysEnemies = [],
                AlwaysFriends = [],
                BotRole = botName,
                ChancedEnemies = [],
                Neutral = []
            };

            List<string> warnList = [];

            foreach ((string otherBot, string otherFaction) in dataConfig.BotFaction)
            {
                BotRelationship relationship = GetFactionRelationship(botFaction, otherFaction);

                switch (relationship)
                {
                    case BotRelationship.Enemies:
                        newSettings.AlwaysEnemies.Add(otherBot);
                        continue;
                    case BotRelationship.Friends:
                        newSettings.AlwaysFriends.Add(otherBot);
                        continue;
                    case BotRelationship.Neutral:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                switch (modConfig.RaidConfig.NeutralityMode)
                {
                    case NeutralMode.ChancedEnemies:
                        newSettings.ChancedEnemies.Add(new ChancedEnemy
                        {
                            EnemyChance = modConfig.RaidConfig.EnemyChance,
                            Role = otherBot
                        });
                        break;
                    case NeutralMode.Warn:
                        warnList.Add(otherBot);
                        break;
                    case NeutralMode.Neutral:
                    default:
                        newSettings.Neutral.Add(otherBot);
                        break;
                }
                
            }

            newSettings.Warn = warnList;
            finalSettings.Add(newSettings);
        }

        _hostilityCache = finalSettings;
    }

    private BotRelationship GetFactionRelationship(string factionName, string otherFaction)
    {
        Faction factionData = dataConfig.Factions[factionName];
        
        if (factionName == otherFaction)
            return BotRelationship.Friends;

        int attitude = factionData.Attitudes.GetValueOrDefault(otherFaction, -1);

        return attitude switch
        {
            1 => BotRelationship.Friends,
            0 => BotRelationship.Neutral,
            _ => BotRelationship.Enemies
        };
    }
}