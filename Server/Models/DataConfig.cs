using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace TerritoryServer.Models;

public record DataConfig
{
    [JsonPropertyName("factions")] public Dictionary<string, Faction> Factions { get; } = [];
    [JsonPropertyName("defaultTerritory")] public LocationData<string> LocationTerritories { get; } = new();
    [JsonPropertyName("botFactionTable")] public Dictionary<string, string> BotFaction { get; } = [];
    [JsonPropertyName("locationNeighbors")] public LocationData<List<string>> LocationNeighbors { get; } = new();
}

public record Faction
{
    [JsonPropertyName("id")] public string Id { get; } = "invalid";
    [JsonPropertyName("color")] public string Color { get; } = "#000000";
    [JsonPropertyName("base")] public string Base { get; } = "none";
    [JsonPropertyName("botNames")] public List<string> BotNames { get; } = [];
    [JsonPropertyName("mobileBosses")] public List<string> BossNames { get; } = [];
    [JsonPropertyName("strength")] public double Strength { get; }
    [JsonPropertyName("defensiveness")] public double Defensiveness { get; }
    [JsonPropertyName("distanceReduction")] public double DistanceReduction { get; }
    [JsonPropertyName("persistant")] public bool Persistant { get; }
    [JsonPropertyName("defaultRep")] public double DefaultRep { get; }
    [JsonPropertyName("gainRep")] public bool RepEnabled { get; }
    [JsonPropertyName("associatedTrader")] public MongoId? Trader { get; }
    [JsonPropertyName("factionAttitude")] public Dictionary<string, int> Attitudes { get; } = [];
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
            "suburbs" => Icebreaker,
            "terminal" => Terminal,
            _ => throw new KeyNotFoundException($"Location '{key}' not found.")
        };
}

