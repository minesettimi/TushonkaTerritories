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
    [JsonPropertyName("killRepDecrease")] public double KillReputationDecrease { get; set; } = 0.01f;
    [JsonPropertyName("killEnemyRep")] public double KillEnemyReputation { get; set; } = 0.005f;
    [JsonPropertyName("overrideTraderRep")] public bool OverrideTraderRep { get; set; } = false;
}

public record BattleConfig
{
    [JsonPropertyName("enableBattles")] public bool BattlesEnabled { get; set; } = true;
    [JsonPropertyName("allowBaseTaking")] public bool BaseTakingEnabled { get; set; } = false;
    [JsonPropertyName("strengthDecreaseOverride")] public double StrengthDecrease { get; set; } = -1f;
    [JsonPropertyName("simulateAfterRaid")] public bool RaidBattle { get; set; } = true;
    [JsonPropertyName("raidsChangeOutcome")] public bool RaidChangesBattle { get; set; } = true;
    [JsonPropertyName("offlineSimulationTime")] public double SimulationInterval { get; set; } = -1f;
    [JsonPropertyName("actionsPerSimulation")] public int SimulationActions { get; set; } = 1;
    [JsonPropertyName("locationsPerSimulation")] public int SimulationLocations { get; set; } = 2;
    [JsonPropertyName("attackNeutralChance")] public double AttackNeutralChance { get; } = 25.0;
    [JsonPropertyName("damageMultiplier")] public double DamageMultiplier { get; } = 1.0;
    [JsonPropertyName("damageMinDistribution")] public double DamageMinRng { get; } = -0.35;
    [JsonPropertyName("damageMaxDistribution")] public double DamageMaxRng { get; } = 0.15;
}

public record RaidConfig
{
    [JsonPropertyName("alwaysHostilePmcs")] public bool HostilePmcs { get; set; } = false;
    [JsonPropertyName("overridePmcs")] public bool OverridePmcs { get; set; } = false;
    [JsonPropertyName("overrideBosses")] public bool OverrideBosses { get; set; } = false;
    [JsonPropertyName("removeDefaultScavs")] public bool OverrideWaves { get; set; } = true;
    [JsonPropertyName("addFactionBosses")] public bool FactionBosses { get; set; } = true;
    [JsonPropertyName("allyRepRequirement")] public double AllyRep { get; set; } = 3f;
    [JsonPropertyName("warnRepRequirement")] public double NeutralRep { get; set; } = 1f;
    [JsonPropertyName("attitudeChangesAllies")] public bool AttitudeEffect { get; set; } = true;
    [JsonPropertyName("neutralityMode")] public NeutralMode NeutralityMode { get; set; } = NeutralMode.Warn;
    [JsonPropertyName("enemyChance")] public int EnemyChance { get; set; } = 50;
    [JsonPropertyName("minStrengthCount")] public int MinBotCount { get; set; } = 10;
    [JsonPropertyName("maxStrengthCount")] public int MaxBotCount { get; set; } = 30;
    [JsonPropertyName("variedBotCounts")] public bool VariedBotCounts { get; set; } = true;
    [JsonPropertyName("roundedBotCounts")] public bool RoundedBotCounts { get; set; } = true;
    [JsonPropertyName("minStrengthUnits")] public int MinStrengthUnits { get; set; } = 1;
    [JsonPropertyName("maxStrengthUnits")] public int MaxStrengthUnits { get; set; } = 4;
    [JsonPropertyName("unitCountVariance")] public bool UnitCountVariance { get; set; } = true;

    [JsonPropertyName("difficultyThresholds")]
    public BotDifficulty DifficultyThresholds { get; set; } = new()
    {
        Impossible = 0.9,
        Hard = 0.7,
        Normal = 0.3
    };
    [JsonPropertyName("difficultyDecreaseChance")] public double DifficultyChance { get; set; } = 50.0f;
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