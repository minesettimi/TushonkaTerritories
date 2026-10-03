using System.Text.Json.Serialization;

namespace TerritoryServer.Models;

public record ModConfig
{
    [JsonPropertyName("debug")] public bool Debug { get; set; } = false;
    [JsonPropertyName("initialSimulations")] public int InitialSimulations { get; set; } = 2;
    [JsonPropertyName("factionConfig")] public FactionConfig FactionConfig { get; set; } = new();
    [JsonPropertyName("battleConfig")] public BattleConfig BattleConfig { get; set; } = new();
    [JsonPropertyName("raidConfig")] public RaidConfig RaidConfig { get; set; } = new();
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
    [JsonPropertyName("enemyChance")] public int EnemyChance { get; set; } = 50;
    [JsonPropertyName("waveDelay")] public MinMax<int> WaveDelay = new()
    {
        Min = 320,
        Max = 460
    };
    [JsonPropertyName("delayVariance")] public int DelayVariance { get; set; } = 5;
    [JsonPropertyName("maxWaves")] public int MaxWaveCap { get; set; } = 12;
    [JsonPropertyName("strengthWaveSize")]
    public MinMax<int> WaveBotCount { get; set; } = new()
    {
        Min = 3,
        Max = 6
    };
    [JsonPropertyName("roundedBotCounts")] public bool RoundedBotCounts { get; set; } = false;
    [JsonPropertyName("strengthUnits")]
    public MinMax<int> StrengthUnits { get; set; } = new()
    {
        Min = 2,
        Max = 4
    };
    [JsonPropertyName("groupSizeVariance")] public int VariedGroupSize { get; set; } = 1;
    [JsonPropertyName("groupChance")] public int GroupChance { get; set; } = 30;
    [JsonPropertyName("spawnEnd")] public int SpawnEnd { get; set; } = 300;
    [JsonPropertyName("initialBotMultiplier")] public double InitialBotMult { get; set; } = 1.5;
    [JsonPropertyName("enforceBotSpawns")] public bool EnforceBotSpawns { get; set; } = false;
    [JsonPropertyName("enforceInitialSpawns")] public bool EnforceFirstWave { get; set; } = false;
    [JsonPropertyName("enforcePmcSpawns")] public bool EnforcePmcSpawns { get; set; } = false;
    
    [JsonPropertyName("difficultyThresholds")]
    public BotDifficulty DifficultyThresholds { get; set; } = new()
    {
        Impossible = 0.9,
        Hard = 0.7,
        Normal = 0.4
    };
    [JsonPropertyName("difficultyDecreaseChance")] public double DifficultyChance { get; set; } = 45.0f;
}

public enum NeutralMode
{
    Warn,
    Neutral,
    ChancedEnemies
}

public record BotDifficulty
{
    [JsonPropertyName("impossible")] public double Impossible { get; init; }
    [JsonPropertyName("hard")] public double Hard { get; init; }
    [JsonPropertyName("normal")] public double Normal { get; init; }
    [JsonPropertyName("easy")] public double Easy { get; init; }

    [JsonIgnore]
    public double this[string key] =>
        key.ToLower() switch
        {
            "impossible" => Impossible,
            "hard" => Hard,
            "normal" => Normal,
            "easy" => Easy,
            _ => throw new KeyNotFoundException($"No difficulty found: {key}")
        };
}