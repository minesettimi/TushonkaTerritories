using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Enums;

namespace TerritoryServer.Models;

public class TerritoryDataConfig
{
    [JsonPropertyName("factions")] public Dictionary<string, Faction> Factions { get; set; } = [];
    [JsonPropertyName("defaultTerritory")] public LocationData<LocationInitialState> LocationTerritories { get; set; } = new();
    [JsonPropertyName("locationNeighbors")] public LocationData<List<string>> LocationNeighbors { get; set; } = new();
    
    //can't do strings because its a list and I don't feel like making my own converter
    [JsonPropertyName("savedQuestRewards")]
    public List<RewardType> SavedQuestRewards { get; set; } = [];
}

public class Faction
{
    [JsonIgnore] public bool Deactivated { get; set; } = false;
    
    [JsonPropertyName("color")] public string Color { get; set; } = "#FFFFFF";
    [JsonPropertyName("base")] public string? Base { get; set; } = null;
    [JsonPropertyName("botNames")] public List<string> BotNames { get; set; } = [];
    [JsonPropertyName("extraBotNames")] public List<string> ExtraBotNames { get; set; } = [];
    [JsonPropertyName("mobileBosses")] public List<string> MobileBossNames { get; set; } = [];
    [JsonPropertyName("staticBosses")] public List<string> ExtraBossNames { get; set; } = [];
    [JsonPropertyName("strength")] public double Strength { get; set; }
    [JsonPropertyName("defensiveness")] public double Defensiveness { get; set; }
    [JsonPropertyName("aggressiveness")] public double Aggressiveness { get; set; }
    [JsonPropertyName("distanceReduction")] public double DistanceReduction { get; set; }
    [JsonPropertyName("maxStrengthBuild")] public double MaxStrengthBuild { get; set; }
    [JsonPropertyName("persistant")] public bool Persistant { get; set; } = false;
    [JsonPropertyName("phantom")] public bool Phantom { get; set; } = false;
    [JsonPropertyName("uprising")] public double UprisingChance { get; set; }
    [JsonPropertyName("defaultRepUsec")] public double DefaultRepUsec { get; set; }
    [JsonPropertyName("defaultRepBear")] public double DefaultRepBear { get; set; }
    [JsonPropertyName("defaultRepScav")] public double DefaultRepScav { get; set; }
    [JsonPropertyName("gainRep")] public bool RepEnabled { get; set; }
    [JsonPropertyName("associatedTrader")] public MongoId? Trader { get; set; }
    [JsonPropertyName("modSupport")] public FactionModData? ModSupport { get; set; }
    [JsonPropertyName("factionAttitude")] public Dictionary<string, int> Attitudes { get; set; } = [];
}

public class LocationInitialState
{
    [JsonPropertyName("holder")] public string Holder { get; set; } = "none";
    [JsonPropertyName("locked")] public bool Locked { get; set; } = false;
    [JsonPropertyName("modLockGuid")] public string? ModLock { get; set; }
}

public class FactionModData
{
    [JsonPropertyName("modGuid")] public required string ModGuid { get; set; }
    [JsonPropertyName("backupFaction")] public string BackupFaction { get; set; } = "none";
}