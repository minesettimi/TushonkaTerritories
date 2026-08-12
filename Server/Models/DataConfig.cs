using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace TerritoryServer.Models;

public record DataConfig
{
    [JsonPropertyName("factions")] public Dictionary<string, Faction> Factions { get; set; } = [];
    [JsonPropertyName("defaultBases")] public LocationData<string> LocationBases { get; set; } = new();
    [JsonPropertyName("botFactionTable")] public Dictionary<string, string> BotFaction { get; set; } = [];
    [JsonPropertyName("locationNeighbors")] public LocationData<List<string>> LocationNeighbors { get; set; } = new();
}

public record Faction
{
    [JsonPropertyName("id")] public string Id { get; set; } = "invalid";
    [JsonPropertyName("color")] public string Color { get; set; } = "#000000";
    [JsonPropertyName("botNames")] public List<string> BotNames { get; set; } = [];
    [JsonPropertyName("mobileBosses")] public List<string> BossNames { get; set; } = [];
    [JsonPropertyName("strength")] public float Strength { get; set; }
    [JsonPropertyName("defensiveness")] public float Defensiveness { get; set; }
    [JsonPropertyName("distanceReduction")] public float DistanceReduction { get; set; }
    [JsonPropertyName("persistant")] public bool Persistant { get; set; }
    [JsonPropertyName("defaultRep")] public float DefaultRep { get; set; }
    [JsonPropertyName("gainRep")] public bool RepEnabled { get; set; }
    [JsonPropertyName("associatedTrader")] public MongoId? Trader { get; set; }
    [JsonPropertyName("factionAttitude")] public Dictionary<string, float> Attitudes { get; set; } = [];
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

    [JsonIgnore]
    public T this[string key] =>
        key.ToLowerInvariant() switch
        {
            "bigmap" => Customs,
            "factory4_day" => Factory,
            "factory4_night" => Factory,
            "interchange" => Interchange,
            "laboratory" => Laboratory,
            "lighthouse" => Lighthouse,
            "reservbase" => Reserve,
            "sandbox" => GroundZero,
            "sandbox_high" => GroundZero,
            "shoreline" => Shoreline,
            "tarkovstreets" => Streets,
            "woods" => Woods,
            "labyrinth" => Labyrinth,
            _ => throw new KeyNotFoundException($"Location '{key}' not found.")
        };
}

