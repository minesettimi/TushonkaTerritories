using System.Text.Json.Serialization;

namespace TerritoryServer.Models;

public record ModConfig
{
    [JsonPropertyName("debug")] public bool Debug { get; } = false;
    [JsonPropertyName("factionConfig")] public FactionConfig FactionConfig { get; set; } = new();
    [JsonPropertyName("battleConfig")] public BattleConfig BattleConfig { get; set; } = new();
    [JsonPropertyName("raidConfig")] public RaidConfig RaidConfig { get; set; } = new();
}

public record FactionConfig
{
    [JsonPropertyName("killChangesRep")] public bool RepChange { get; set; } = true;
    [JsonPropertyName("killRepDecrease")] public double KillReputationDecrease { get; set; } = 0.01;
    [JsonPropertyName("killEnemyRep")] public double KillEnemyReputation { get; set; } = 0.005;
    [JsonPropertyName("overrideTraderRep")] public bool OverrideTraderRep { get; set; } = false;
}

public record BattleConfig
{
    [JsonPropertyName("enableBattles")] public bool BattlesEnabled { get; set; } = true;
    [JsonPropertyName("allowBaseTaking")] public bool BaseTakingEnabled { get; set; } = false;
    [JsonPropertyName("strengthDecreaseOverride")] public double StrengthDecrease { get; set; } = -1f;
    [JsonPropertyName("simulateAfterRaid")] public bool RaidBattle { get; set; } = true;
    [JsonPropertyName("raidsChangeOutcome")] public bool RaidChangesBattle { get; set; } = true;
    [JsonPropertyName("strengthLossPerDeath")] public double RaidStrengthLoss { get; set; } = 0.01;
    [JsonPropertyName("offlineSimulationTime")] public double SimulationInterval { get; set; } = -1f;
    [JsonPropertyName("actionsPerSimulation")] public int SimulationActions { get; set; } = 1;
    [JsonPropertyName("locationsPerSimulation")] public int SimulationLocations { get; set; } = 2;
    [JsonPropertyName("attackNeutralChance")] public double AttackNeutralChance { get; set; } = 25.0;
    [JsonPropertyName("damageMultiplier")] public double DamageMultiplier { get; set; } = 1.0;
    [JsonPropertyName("damageMinDistribution")] public double DamageMinRng { get; set; } = -0.35;
    [JsonPropertyName("damageMaxDistribution")] public double DamageMaxRng { get; set; } = 0.15;
}

public record RaidConfig
{
    [JsonPropertyName("overridePmcs")] public bool OverridePmcs { get; set; } = false;
    [JsonPropertyName("overrideBosses")] public bool OverrideBosses { get; set; } = false;
    [JsonPropertyName("removeDefaultScavs")] public bool OverrideWaves { get; set; } = true;
    [JsonPropertyName("addFactionBosses")] public bool FactionBosses { get; set; } = true;
    [JsonPropertyName("factionBossMinStrength")] public double MinBossStrength { get; set; } = 0.5;
    [JsonPropertyName("allyRepRequirement")] public double AllyRep { get; set; } = 3f;
    [JsonPropertyName("warnRepRequirement")] public double NeutralRep { get; set; } = 1f;
    [JsonPropertyName("attitudeChangesAllies")] public bool AttitudeEffect { get; set; } = true;
    [JsonPropertyName("neutralityMode")] public NeutralMode NeutralityMode { get; set; } = NeutralMode.Warn;
    [JsonPropertyName("enemyChance")] public int EnemyChance { get; set; } = 50;
    [JsonPropertyName("minStrengthWaveDelay")] public int MinWaveDelay { get; set; } = 200;
    [JsonPropertyName("maxStrengthWaveDelay")] public int MaxWaveDelay { get; set; } = 400;
    [JsonPropertyName("delayVariance")] public int DelayVariance { get; set; } = 5;
    [JsonPropertyName("minStrengthWaveSize")] public int MinWaveBotCount { get; set; } = 5;
    [JsonPropertyName("maxStrengthWaveSize")] public int MaxWaveBotCount { get; set; } = 16;
    [JsonPropertyName("roundedBotCounts")] public bool RoundedBotCounts { get; set; } = true;
    [JsonPropertyName("minStrengthGroupSize")] public int MinStrengthUnits { get; set; } = 1;
    [JsonPropertyName("maxStrengthGroupSize")] public int MaxStrengthUnits { get; set; } = 4;
    [JsonPropertyName("groupSizeMinCount")] public int VariedGroupSize { get; set; } = 3;
    [JsonPropertyName("spawnEnd")] public int SpawnEnd { get; set; } = 300;
    [JsonPropertyName("initialBotMultiplier")] public double InitialBotMult { get; set; } = 2;

    [JsonPropertyName("difficultyThresholds")]
    public BotDifficulty DifficultyThresholds { get; set; } = new()
    {
        Impossible = 0.9,
        Hard = 0.7,
        Normal = 0.3
    };
    [JsonPropertyName("difficultyDecreaseChance")] public double DifficultyChance { get; set; } = 25.0f;
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