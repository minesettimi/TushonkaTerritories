using System.Collections.Generic;
using EFT;
using Newtonsoft.Json;

namespace TerritoryClient.Models;

public class ServerStateData
{
    [JsonProperty("locations")] public LocationData<LocationState?> Locations { get; set; } = null!;
    [JsonProperty("playerRep")] public Dictionary<MongoID, Dictionary<string, double>> PlayerRep { get; set; } = null!;
    [JsonProperty("factionColors")] public Dictionary<string, string> FactionColors { get; set; } = null!;
    [JsonProperty("allyRep")] public double AllyRep { get; set; }
    [JsonProperty("neutralRep")] public double NeutralRep { get; set; }
}

public record LocationState
{
    [JsonProperty("holder")] public string Holder { get; set; } = null!;
    [JsonProperty("contestants")] public Dictionary<string, double> Contestants { get; set; } = null!;
    [JsonProperty("base")] public bool Base { get; set; }
}

public class LocationData<T>
{
    [JsonProperty("bigmap")] public T Customs { get; set; }
    [JsonProperty("factory4_day")] public T Factory { get; set; }
    [JsonProperty("interchange")] public T Interchange { get; set; }
    [JsonProperty("laboratory")] public T Laboratory { get; set; }
    [JsonProperty("lighthouse")] public T Lighthouse { get; set; }
    [JsonProperty("rezervbase")] public T Reserve { get; set; }
    [JsonProperty("sandbox")] public T GroundZero { get; set; }
    [JsonProperty("shoreline")] public T Shoreline { get; set; }
    [JsonProperty("tarkovstreets")] public T Streets { get; set; }
    [JsonProperty("woods")] public T Woods { get; set; }
    [JsonProperty("labyrinth")] public T Labyrinth { get; set; }
    [JsonProperty("suburbs")] public T Icebreaker { get; set; }
    [JsonProperty("terminal")] public T Terminal { get; set; }

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
