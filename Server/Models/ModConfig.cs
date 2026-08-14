using System.Text.Json.Serialization;

namespace TerritoryServer.Models;

public record ModConfig
{
    [JsonPropertyName("factionConfig")] public FactionConfig Factions { get; set; } = new();
    [JsonPropertyName("battleConfig")] public BattleConfig BattleConfig { get; set; } = new();
}

public record FactionConfig
{
    [JsonPropertyName("killRepDecrease")] public double KillReputationDecrease { get; set; } = 0.01f;
    [JsonPropertyName("killEnemyRep")] public double KillEnemyReputation { get; set; } = 0.005f;
    [JsonPropertyName("overrideTraderRep")] public bool OverrideTraderRep { get; set; } = false;
    [JsonPropertyName("changeAttitude")] public bool ChangeAttitude { get; set; } = false;
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
    [JsonPropertyName("overrideWaves")] public bool OverrideWaves { get; set; } = true;
    [JsonPropertyName("overrideBosses")] public bool OverrideBosses { get; set; } = true;
    [JsonPropertyName("overridePmcs")] public bool OverridePmcs { get; set; } = false;
    [JsonPropertyName("allyRepRequirement")] public double AllyRep { get; set; } = 5f;
    [JsonPropertyName("attitudeChangesAllies")] public bool AttitudeEffect { get; set; } = true;
    [JsonPropertyName("partialAttitude")] public bool PartialAttitude { get; set; } = true;
    [JsonPropertyName("minStrengthWaves")] public int MinStengthWaves { get; set; } = 1;
    [JsonPropertyName("maxStrengthWaves")] public int MaxStrengthWaves { get; set; } = 6;
    [JsonPropertyName("minStrengthUnits")] public int MinStrengthUnits { get; set; } = 1;
    [JsonPropertyName("maxStrengthUnits")] public int MaxStrengthUnits { get; set; } = 4;
    [JsonPropertyName("unitCountVariance")] public bool UnitCountVariance { get; set; } = true;
}