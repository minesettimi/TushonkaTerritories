using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;

namespace TerritoryServer.Models;

public class DataConfig
{
    [JsonPropertyName("factions")] public Dictionary<string, Faction> Factions { get; set; } = [];
    [JsonPropertyName("defaultTerritory")] public LocationData<LocationInitialState> LocationTerritories { get; set; } = new();
    [JsonPropertyName("locationNeighbors")] public LocationData<List<string>> LocationNeighbors { get; set; } = new();
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

//Credit to acidphantasm for the base of this better strategy of mapping locations
public class LocationData<T>
{
    [JsonPropertyName("bigmap")] public T Customs { get; set; } = default!;
    [JsonPropertyName("factory4_day")] public T Factory { get; set; } = default!;
    [JsonPropertyName("interchange")] public T Interchange { get; set; } = default!;
    [JsonPropertyName("laboratory")] public T Laboratory { get; set; } = default!;
    [JsonPropertyName("lighthouse")] public T Lighthouse { get; set; } = default!;
    [JsonPropertyName("rezervbase")] public T Reserve { get; set; } = default!;
    [JsonPropertyName("sandbox")] public T GroundZero { get; set; } = default!;
    [JsonPropertyName("shoreline")] public T Shoreline { get; set; } = default!;
    [JsonPropertyName("tarkovstreets")] public T Streets { get; set; } = default!;
    [JsonPropertyName("woods")] public T Woods { get; set; } = default!;
    [JsonPropertyName("labyrinth")] public T Labyrinth { get; set; } = default!;
    [JsonPropertyName("icebreaker")] public T Icebreaker { get; set; } = default!;
    [JsonPropertyName("terminal")] public T Terminal { get; set; } = default!;

    [JsonIgnore]
    public T this[string key]
    {
        get => key.ToLowerInvariant() switch
        {
            "bigmap" => Customs,
            "factory4_day" => Factory,
            "factory4_night" => Factory,
            "interchange" => Interchange,
            "laboratory" => Laboratory,
            "lighthouse" => Lighthouse,
            "rezervbase" => Reserve,
            "sandbox" => GroundZero,
            "sandbox_high" => GroundZero,
            "shoreline" => Shoreline,
            "tarkovstreets" => Streets,
            "woods" => Woods,
            "labyrinth" => Labyrinth,
            "icebreaker" => Icebreaker,
            "terminal" => Terminal,
            _ => throw new KeyNotFoundException($"Location '{key}' not found.")
        };
        set
        {
            switch (key.ToLowerInvariant())
            {
                case "bigmap": Customs = value; break;
                case "factory4_day":
                case "factory4_night": Factory = value; break;
                case "interchange": Interchange = value; break;
                case "lighthouse": Lighthouse = value; break;
                case "rezervbase": Reserve = value; break;
                case "sandbox":
                case "sandbox_high": GroundZero = value; break;
                case "shoreline": Shoreline = value; break;
                case "tarkovstreets": Streets = value; break;
                case "woods": Woods = value; break;
                case "laboratory": Laboratory = value; break;
                case "labyrinth": Labyrinth = value; break;
                case "icebreaker": Icebreaker = value; break;
                case "terminal": Terminal = value; break;
                default: throw new KeyNotFoundException($"Location '{key}' not found.");
            }
        }
    }
}

