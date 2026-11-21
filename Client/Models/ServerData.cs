using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EFT;
using Newtonsoft.Json;
using TerritoryClient.Services;
using UnityEngine;

namespace TerritoryClient.Models;

public class ServerData
{
    [JsonProperty("factions")] public Dictionary<string, FactionData> Factions { get; set; } = null!;
    [JsonProperty("botFactionTable")] public Dictionary<string, string> BotFaction { get; set; } = [];
    [JsonProperty("spawnSettings")] public SpawnSettings SpawnSettings { get; set; } = null!;
    [JsonProperty("mapList")] public List<string> MapList { get; set; } = [];
    [JsonProperty("duplicateMapList")] public Dictionary<string, string> DuplicateMapList { get; set; } = [];
    [JsonProperty("presetList")] public List<string> PresetList { get; set; } = [];
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
public class LocationData<T> : Dictionary<string, T>
{
    public T? this[string key]
    {
        get
        {
            string adjustedKey = key.ToLowerInvariant();
            if (StateManager.DuplicateMapIndex.TryGetValue(adjustedKey, out string? newKey))
            {
                adjustedKey = newKey;
            }

            return TryGetValue(adjustedKey, out T? value) ? value : default;
        }
        set
        {
            string adjustedKey = key.ToLowerInvariant();
            if (StateManager.DuplicateMapIndex.TryGetValue(adjustedKey, out string? newKey))
            {
                adjustedKey = newKey;
            }
            
            base[adjustedKey] = value!;
        }
    }
}