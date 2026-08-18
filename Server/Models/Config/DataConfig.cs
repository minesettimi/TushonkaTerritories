using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace TerritoryServer.Models;

public class DataConfig
{
    [JsonPropertyName("factions")] public Dictionary<string, Faction> Factions { get; set; } = [];
    [JsonPropertyName("defaultTerritory")] public LocationData<string> LocationTerritories { get; set; } = new();
    [JsonPropertyName("botFactionTable")] public Dictionary<string, string> BotFaction { get; set; } = [];
    [JsonPropertyName("locationNeighbors")] public LocationData<List<string>> LocationNeighbors { get; set; } = new();
}

public class Faction
{
    [JsonPropertyName("color")] public string Color { get; set; } = "#000000";
    [JsonPropertyName("base")] public string? Base { get; set; } = null;
    [JsonPropertyName("botNames")] public List<string> BotNames { get; set; } = [];
    [JsonPropertyName("mobileBosses")] public List<string> BossNames { get; set; } = [];
    [JsonPropertyName("strength")] public double Strength { get; set; }
    [JsonPropertyName("defensiveness")] public double Defensiveness { get; set; }
    [JsonPropertyName("distanceReduction")] public double DistanceReduction { get; set; }
    [JsonPropertyName("persistant")] public bool Persistant { get; set; }
    [JsonPropertyName("defaultRepUsec")] public double DefaultRepUsec { get; set; }
    [JsonPropertyName("defaultRepBear")] public double DefaultRepBear { get; set; }
    [JsonPropertyName("defaultRepScav")] public double DefaultRepScav { get; set; }
    [JsonPropertyName("gainRep")] public bool RepEnabled { get; set; }
    [JsonPropertyName("associatedTrader")] public MongoId? Trader { get; set; }
    [JsonPropertyName("factionAttitude")] public Dictionary<string, int> Attitudes { get; set; } = [];
}

//Credit to acidphantasm for the base of this better strategy of mapping locations
public class LocationData<T>
{
    [JsonPropertyName("bigmap")] public T Customs { get; set; }
    [JsonPropertyName("factory4_day")] public T Factory { get; set; }
    [JsonPropertyName("interchange")] public T Interchange { get; set; }
    [JsonPropertyName("laboratory")] public T Laboratory { get; set; }
    [JsonPropertyName("lighthouse")] public T Lighthouse { get; set; }
    [JsonPropertyName("rezervbase")] public T Reserve { get; set; }
    [JsonPropertyName("sandbox")] public T GroundZero { get; set; }
    [JsonPropertyName("shoreline")] public T Shoreline { get; set; }
    [JsonPropertyName("tarkovstreets")] public T Streets { get; set; }
    [JsonPropertyName("woods")] public T Woods { get; set; }
    [JsonPropertyName("labyrinth")] public T Labyrinth { get; set; }
    [JsonPropertyName("suburbs")] public T Icebreaker { get; set; }
    [JsonPropertyName("terminal")] public T Terminal { get; set; }

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
            "suburbs" => Icebreaker,
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
                case "suburbs": Icebreaker = value; break;
                case "terminal": Terminal = value; break;
                default: throw new KeyNotFoundException($"Location '{key}' not found.");
            }
        }
    }
}

