using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EFT;
using Newtonsoft.Json;
using UnityEngine;

namespace TerritoryClient.Models;

public class ServerData
{
    [JsonProperty("factions")] public Dictionary<string, FactionData> Factions { get; set; } = null!;
    [JsonProperty("botFactionTable")] public Dictionary<string, string> BotFaction { get; set; } = [];
    [JsonProperty("spawnSettings")] public SpawnSettings SpawnSettings { get; set; } = null!;
    [JsonProperty("attitudeEffect")] public bool AttitudeEffect { get; set; }
    [JsonProperty("allyRep")] public double AllyRep { get; set; }
    [JsonProperty("neutralRep")] public double NeutralRep { get; set; }
}

public class FactionData
{
    [JsonProperty("color")] public string FactionColor { get; set; } = null!;
    [JsonProperty("bots")] public List<string> BotNames { get; set; } = [];
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
                TerritoryPlugin.PluginLogger.LogError($"Failed to parse color {FactionColor}!");
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

    [JsonIgnore]
    public WildSpawnType[] BotTypes
    {
        get
        {
            if (field == null)
            {
                List<WildSpawnType> spawnTypes = [];
                foreach (string botName in BotNames)
                {
                    if (!Enum.TryParse(botName, out WildSpawnType wildSpawnType))
                    {
                        throw new Exception($"[TT] Failed to parse bot type {botName}!");
                    }
                    
                    spawnTypes.Add(wildSpawnType);
                }
                
                field = [.. spawnTypes];
            }
            
            return field;
        }
    }
}

public class ServerState
{
    [JsonProperty("stateId")] public MongoID StateId { get; set; }
    [JsonProperty("lastSimulatedLoc")] public int LastLoc { get; set; } = 0;
    [JsonProperty("locations")] public LocationData<LocationState> Locations { get; set; } = null!;
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

public record SpawnSettings
{
    [JsonProperty("perBotLoadout")] public int PerBotLoadout { get; set; }
    [JsonProperty("extraLoadouts")] public int ExtraLoadouts { get; set; }
    [JsonProperty("difficultyWeights")] public Dictionary<double, Dictionary<BotDifficulty, int>>? DifficultyWeights { get; set; } = [];
    [JsonProperty("strengthGroupSizeMin")] public MinMax<int> GroupSizeMin { get; set; } = null!;
    [JsonProperty("strengthGroupSizeMax")] public MinMax<int> GroupSizeMax { get; set; } = null!;
    [JsonProperty("strengthGroupChance")] public MinMax<int> GroupChance { get; set; } = null!;
    [JsonProperty("nonGroupBotModifier")] public float NonGroupBotModifier { get; set; }
    [JsonProperty("groupSpawnExclusive")] public bool GroupSpawnExclusive { get; set; }
    [JsonProperty("startTime")] public int StartTime { get; set; }
    [JsonProperty("stopTime")] public int EndTime { get; set; }
    [JsonProperty("requiredCapSpace")] public int RequiredSpawnSpace { get; set; }
    [JsonProperty("softCapSpace")] public int SoftCapSpace { get; set; }
    [JsonProperty("mapSettings")] public LocationData<LocationSpawnSettings> MapSettings { get; set; } = new();
    [JsonProperty("spawnCheckInterval")] public float SpawnCheck { get; set; }
    [JsonProperty("spawnDelayAdjustmentMax")] public float SpawnDelayAdjustmentMax { get; set; }
    [JsonProperty("maxBotSpawnsPerInterval")] public int MaxIntervalSpawns { get; set; }
    [JsonProperty("deadRaidThreshold")] public int DeadRaidBots { get; set; }
    [JsonProperty("deadRaidTimer")] public float DeadRaidTime { get; set; }
}

public record LocationSpawnSettings
{
    [JsonProperty("maxBots")] public int MaxBots { get; set; }
    [JsonProperty("spawnOnWindow")] public MinMax<float> SpawnOnWindow { get; set; } = null!;
    [JsonProperty("spawnOffWindow")] public MinMax<float> SpawnOffWindow { get; set; } = null!;
}

public record MinMax<T> where T : IComparable
{
    [JsonProperty("min")] public T Min { get; set; }
    [JsonProperty("max")] public T Max { get; set; }
}

//convert this back to generic at some point if needed
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
    [JsonProperty("icebreaker")] public T Icebreaker { get; set; }
    [JsonProperty("terminal")] public T Terminal { get; set; }

    [JsonIgnore]
    public T? this[string key] =>
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
            "icebreaker" => Icebreaker,
            "terminal" => Terminal,
            _ => throw new ArgumentOutOfRangeException()
        };
}

public static class LocationInfo
{
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
        "icebreaker",
        "terminal"
    ];
}