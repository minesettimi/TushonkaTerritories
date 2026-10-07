using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums.RaidSettings;

namespace TerritoryServer.Models;

public record TerritoryModConfig
{
    [JsonPropertyName("debug")] public bool Debug { get; set; } = false;
    [JsonPropertyName("initialSimulations")] public int InitialSimulations { get; set; } = 2;
    [JsonPropertyName("factionConfig")] public FactionConfig FactionConfig { get; set; } = new();
    [JsonPropertyName("battleConfig")] public BattleConfig BattleConfig { get; set; } = new();
    [JsonPropertyName("raidConfig")] public RaidConfig RaidConfig { get; set; } = new();
    [JsonPropertyName("spawnSettings")] public SpawnSettings SpawnSettings { get; set; } = new();
}

public record FactionConfig
{
    [JsonPropertyName("killChangesRep")] public bool RepChange { get; set; } = true;
    [JsonPropertyName("killRepDecrease")] public double KillReputationDecrease { get; set; } = 0.005;
    [JsonPropertyName("killEnemyRep")] public double KillEnemyReputation { get; set; } = 0.05;
    [JsonPropertyName("overrideTraderRep")] public bool TraderReputation { get; set; } = false;
}

public record BattleConfig
{
    [JsonPropertyName("enableBattles")] public bool BattlesEnabled { get; set; } = true;
    [JsonPropertyName("allowBaseTaking")] public bool BaseTakingEnabled { get; set; } = false;
    [JsonPropertyName("strengthDecreaseOverride")] public double StrengthDecrease { get; set; } = -1f;
    [JsonPropertyName("simulateAfterRaid")] public bool RaidBattle { get; set; } = true;
    [JsonPropertyName("raidsChangeOutcome")] public bool RaidChangesBattle { get; set; } = true;
    [JsonPropertyName("strengthLossPerDeath")] public double RaidStrengthLoss { get; set; } = 0.05;
    [JsonPropertyName("strengthLossPerBossDeath")] public double RaidBossStrengthLoss { get; set; } = 0.2;
    [JsonPropertyName("offlineSimulationTime")] public int SimulationInterval { get; set; } = -1;
    [JsonPropertyName("actionsPerSimulation")] public int SimulationActions { get; set; } = 2;
    [JsonPropertyName("locationsPerSimulation")] public int SimulationLocations { get; set; } = 4;
    [JsonPropertyName("attackNeutralChance")] public double AttackNeutralChance { get; set; } = 25.0;
    [JsonPropertyName("damageMultiplier")] public double DamageMultiplier { get; set; } = 1.2;
    [JsonPropertyName("damageDistribution")]
    public MinMax<double> DamageRng { get; set; } = new()
    {
        Min = -0.35,
        Max = 0.35
    };
    [JsonPropertyName("strengthBuildup")] public double StrengthBuildup { get; set; } = 0.05;
    [JsonPropertyName("maxStrengthBuildup")] public double MaxStrengthBuildup { get; set; } = 0.6;
    [JsonPropertyName("spreadMinStrength")] public double SpreadMinStrength { get; set; } = 0.45;
    [JsonPropertyName("spreadDecreaseMult")] public double SpreadMult { get; set; } = 0.25;
    [JsonPropertyName("spreadBonusStrength")] public double SpreadBonus { get; set; } = 0.05;
    [JsonPropertyName("uprising")] public bool Uprising { get; set; } = true;
    [JsonPropertyName("uprisingStrengthMult")] public double UprisingMult { get; set; } = 0.35;
}

public record RaidConfig
{
    [JsonPropertyName("overridePmcs")] public bool OverridePmcs { get; set; } = false;
    [JsonPropertyName("overrideCultists")] public bool OverrideCultists { get; set; } = true;
    [JsonPropertyName("overrideBosses")] public bool OverrideBosses { get; set; } = false;
    [JsonPropertyName("overrideTriggeredSpawns")] public bool OverrideTriggeredSpawns { get; set; } = false;
    [JsonPropertyName("removeDefaultScavs")] public bool OverrideWaves { get; set; } = true;
    [JsonPropertyName("addFactionBosses")] public bool FactionBosses { get; set; } = true;
    [JsonPropertyName("factionBossMinStrength")] public double MinBossStrength { get; set; } = 0.6;
    [JsonPropertyName("factionBossChance")]
    public MinMax<int> BossChance { get; set; } = new()
    {
        Min = 15,
        Max = 25
    };
    [JsonPropertyName("allyRepRequirement")] public double AllyRep { get; set; } = 3f;
    [JsonPropertyName("warnRepRequirement")] public double NeutralRep { get; set; } = 1f;
    [JsonPropertyName("attitudeChangesAllies")] public bool AttitudeEffect { get; set; } = true;
    [JsonPropertyName("playerRepAttitude")] public bool AttitudeEffectPlayer { get; set; } = true;
    [JsonPropertyName("neutralityMode")] public NeutralMode NeutralityMode { get; set; } = NeutralMode.Neutral;
    [JsonPropertyName("overrideHostility")] public bool OverrideHostility { get; set; } = true;
    [JsonPropertyName("enemyChance")] public int EnemyChance { get; set; } = 50;
    [JsonPropertyName("enforcePmcSpawns")] public bool EnforcePmcSpawns { get; set; } = false;
}

public enum NeutralMode
{
    Warn,
    Neutral,
    ChancedEnemies
}

public record SpawnSettings
{
    [JsonPropertyName("perBotLoadout")] public int PerBotLoadout { get; set; } = 12;
    [JsonPropertyName("extraLoadouts")] public int ExtraLoadouts { get; set; } = 8;
    [JsonPropertyName("difficultyWeights")]
    public Dictionary<double, Dictionary<BotDifficulty, int>>?
        DifficultyWeights { get; set; } = new()
    {
        {
            0.9, new Dictionary<BotDifficulty, int>
            {
                { BotDifficulty.Easy, 10 },
                { BotDifficulty.Medium, 20 },
                { BotDifficulty.Hard, 40 },
                { BotDifficulty.Impossible, 30 }
            }
        },
        {
            0.6, new Dictionary<BotDifficulty, int>
            {
                { BotDifficulty.Easy, 20 },
                { BotDifficulty.Medium, 30 },
                { BotDifficulty.Hard, 40 },
                { BotDifficulty.Impossible, 10 }
            }
        },
        {
            0.3, new Dictionary<BotDifficulty, int>
            {
                { BotDifficulty.Easy, 30 },
                { BotDifficulty.Medium, 40 },
                { BotDifficulty.Hard, 20 },
                { BotDifficulty.Impossible, 10 }
            }
        },
        {
            0.0, new Dictionary<BotDifficulty, int>
            {
                { BotDifficulty.Easy, 60 },
                { BotDifficulty.Medium, 30 },
                { BotDifficulty.Hard, 10 },
                { BotDifficulty.Impossible, 0 }
            }
        }
    };

    [JsonPropertyName("strengthGroupSizeMin")]
    public MinMax<int> GroupSizeMin { get; set; } = new()
    {
        Min = 2,
        Max = 4
    };

    [JsonPropertyName("strengthGroupSizeMax")]
    public MinMax<int> GroupSizeMax { get; set; } = new()
    {
        Min = 4,
        Max = 6
    };

    [JsonPropertyName("strengthGroupChance")]
    public MinMax<int> GroupChance { get; set; } = new()
    {
        Min = 20,
        Max = 80
    };

    [JsonPropertyName("nonGroupBotModifier")] public float NonGroupBotModifier { get; set; } = 0.75f;
    [JsonPropertyName("groupSpawnExclusive")] public bool GroupSpawnExclusive { get; set; } = false;
    [JsonPropertyName("startTime")] public int StartTime { get; set; } = 1;
    [JsonPropertyName("stopTime")] public int EndTime { get; set; } = 200; //time before the end of the raid
    [JsonPropertyName("requiredCapSpace")] public int RequiredSpawnSpace { get; set; } = 4;

    [JsonPropertyName("mapSettings")]
    public LocationData<LocationSpawnSettings> MapSettings { get; set; } = new()
    {
        Factory = new LocationSpawnSettings
        {
            MaxBots = 12
        },
        GroundZero = new LocationSpawnSettings
        {
            MaxBots = 14
        },
        Reserve = new LocationSpawnSettings
        {
            MaxBots = 20
        },
        Customs = new LocationSpawnSettings
        {
            MaxBots = 21,
            SpawnOnWindow = new MinMax<float>
            {
                Min = 80f,
                Max = 120f
            }
        },
        Woods = new LocationSpawnSettings
        {
            MaxBots  = 20,
            SpawnOnWindow = new MinMax<float>
            {
                Min = 40f,
                Max = 80f
            }
        },
        Interchange = new LocationSpawnSettings
        {
            MaxBots = 20,
            SpawnOnWindow = new MinMax<float>
            {
                Min = 60f,
                Max = 80f
            },
            SpawnOffWindow = new MinMax<float>
            {
                Min = 280f,
                Max = 360f
            }
        },
        Streets = new LocationSpawnSettings
        {
            MaxBots = 23,
            SpawnOnWindow = new MinMax<float>
            {
                Min = 60f,
                Max = 120f
            },
            SpawnOffWindow = new MinMax<float>
            {
                Min = 280f,
                Max = 360f
            }
        },
        Lighthouse = new LocationSpawnSettings
        {
            MaxBots = 20,
            SpawnOnWindow = new MinMax<float>
            {
                Min = 80f,
                Max = 120f
            }
        },
        Shoreline = new LocationSpawnSettings
        {
            MaxBots = 21,
            SpawnOnWindow = new MinMax<float>
            {
                Min = 80f,
                Max = 120f
            }
        },
        Laboratory = new LocationSpawnSettings
        {
            MaxBots = 15
        },
        Terminal = new LocationSpawnSettings
        {
            MaxBots = 18
        },
        Icebreaker = new LocationSpawnSettings
        {
            MaxBots = 18
        },
        Labyrinth = new LocationSpawnSettings
        {
            MaxBots = 10
        }
    };

    [JsonPropertyName("softCapSpace")] public int SoftCapSpace { get; set; } = 4;
    [JsonPropertyName("spawnCheckInterval")] public float SpawnCheck { get; set; } = 20f;
    [JsonPropertyName("spawnDelayAdjustmentMax")] public float SpawnDelayAdjustmentMax { get; set; } = 2;
    [JsonPropertyName("maxBotSpawnsPerInterval")] public int MaxIntervalSpawns { get; set; } = 8;
    [JsonPropertyName("deadRaidThreshold")] public int DeadRaidThreshold { get; set; } = 4;
    [JsonPropertyName("deadRaidTimer")] public float DeadRaidTimer { get; set; } = 15f;
}

public record LocationSpawnSettings
{
    [JsonPropertyName("maxBots")]
    public int MaxBots { get; set; } = 20;
    
    [JsonPropertyName("spawnOnWindow")]
    public MinMax<float> SpawnOnWindow { get; set; } = new()
    {
        Min = 60,
        Max = 100
    };

    [JsonPropertyName("spawnOffWindow")]
    public MinMax<float> SpawnOffWindow { get; set; } = new()
    {
        Min = 300,
        Max = 380
    };
}