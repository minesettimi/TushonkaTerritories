using System.Text.Json.Serialization;

namespace TerritoryServer.Models;

public record MinMax<T> where T : IComparable<T>
{
    [JsonPropertyName("min")] public required T Min { get; set; }
    [JsonPropertyName("max")] public required T Max { get; set; }
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

