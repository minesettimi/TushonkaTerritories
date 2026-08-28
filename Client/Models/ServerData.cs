using System.Collections.Generic;
using System.Threading.Tasks;
using EFT;
using EFT.Communications;
using Newtonsoft.Json;
using UnityEngine;

namespace TerritoryClient.Models;

public class ServerData
{
    [JsonProperty("factions")] public Dictionary<string, FactionData> Factions { get; set; } = null!;
    [JsonProperty("botFactionTable")] public Dictionary<string, string> BotFaction { get; set; } = [];
    [JsonProperty("attitudeEffect")] public bool AttitudeEffect { get; set; }
    [JsonProperty("allyRep")] public double AllyRep { get; set; }
    [JsonProperty("neutralRep")] public double NeutralRep { get; set; }

}

public class FactionData
{
    [JsonProperty("color")] public string FactionColor { get; set; } = null!;
    [JsonProperty("locked")] public bool Locked { get; set; }

    [JsonIgnore] private Color? _cachedColor;
    [JsonIgnore] public Sprite? Sprite;

    [JsonIgnore]
    public Color Color
    {
        get
        {
            if (_cachedColor != null) return (Color)_cachedColor;
        
            if (!ColorUtility.TryParseHtmlString(FactionColor, out Color colorObj))
            {
                Plugin.PluginLogger.LogError($"Failed to parse color {FactionColor}!");
                return Color.red;
            }

            _cachedColor = colorObj;
            return colorObj;
        }
    }

    public async Task LoadSprite(IImageLoader session, string factionName)
    {
        Sprite sprite = await Utils.LoadIconSprite(session, $"/files/factions/icon/{factionName}");
        Sprite = sprite;
    }
}

public class ServerState
{
    [JsonProperty("stateId")] public MongoID StateId { get; set; }
    [JsonProperty("lastSimulatedLoc")] public int LastLoc { get; set; } = 0;
    [JsonProperty("locations")] public LocationData Locations { get; set; } = null!;
    [JsonProperty("playerState")] public Dictionary<MongoID, PlayerState> PlayerState { get; set; } = null!;

    public double GetPlayerRep(MongoID player, string faction)
    {
        if (PlayerState.TryGetValue(player, out PlayerState playerState) &&
            playerState.Reputation.TryGetValue(faction, out double repValue))
        {
            return repValue;
        }

        return -1.0;
    }
}

public record LocationState
{
    [JsonProperty("holder")] public string Holder { get; set; } = null!;
    [JsonProperty("contestants")] public Dictionary<string, double> Contestants { get; set; } = null!;
    [JsonProperty("base")] public bool Base { get; set; }
}

public record PlayerState
{
    [JsonProperty("reputation")] public Dictionary<string, double> Reputation = [];
    [JsonProperty("unlocked")] public Dictionary<string, bool> Unlocked = [];
}

//convert this back to generic at some point if needed
public class LocationData
{
    [JsonProperty("bigmap")] public LocationState Customs { get; set; }
    [JsonProperty("factory4_day")] public LocationState Factory { get; set; }
    [JsonProperty("interchange")] public LocationState Interchange { get; set; }
    [JsonProperty("laboratory")] public LocationState Laboratory { get; set; }
    [JsonProperty("lighthouse")] public LocationState Lighthouse { get; set; }
    [JsonProperty("rezervbase")] public LocationState Reserve { get; set; }
    [JsonProperty("sandbox")] public LocationState GroundZero { get; set; }
    [JsonProperty("shoreline")] public LocationState Shoreline { get; set; }
    [JsonProperty("tarkovstreets")] public LocationState Streets { get; set; }
    [JsonProperty("woods")] public LocationState Woods { get; set; }
    [JsonProperty("labyrinth")] public LocationState Labyrinth { get; set; }
    [JsonProperty("suburbs")] public LocationState Icebreaker { get; set; }
    [JsonProperty("terminal")] public LocationState Terminal { get; set; }

    [JsonIgnore]
    public LocationState? this[string key] =>
        key.ToLowerInvariant() switch
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
            _ => null
        };

    //I hate to make more hardcoded map strings but its still a quick fix and way better than a more serious system
    public static readonly string[] ValidMaps = 
    [
        "bigmap", 
        "factory4_day", 
        "factory4_night", 
        "interchange", 
        "laboratory", 
        "lighthouse",
        "rezervbase",
        "sandbox",
        "sandbox_high",
        "shoreline",
        "tarkovstreets",
        "woods",
        "labyrinth",
        "suburbs",
        "terminal"
    ];
}
