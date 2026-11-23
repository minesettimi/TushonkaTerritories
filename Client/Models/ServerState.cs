using System.Collections.Generic;
using EFT;
using Newtonsoft.Json;
using SPT.Reflection.Utils;

namespace TerritoryClient.Models;

public class ServerState
{
    [JsonProperty("stateId")] public MongoID StateId { get; set; }
    [JsonProperty("locations")] public LocationData<LocationState> Locations { get; set; } = null!;
    [JsonProperty("playerState")] public Dictionary<MongoID, PlayerState> PlayerState { get; set; } = null!;

    [JsonIgnore] private static MongoID? _playerId;
    [JsonIgnore]
    public static MongoID PlayerId
    {
        get
        {
            _playerId ??= ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.ProfileId;

            return (MongoID)_playerId!;
        }
    }

    public string? SelectedFaction
    {
        get => PlayerState[PlayerId].SelectedFaction;
        set => PlayerState[PlayerId].SelectedFaction = value;
    }
    
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
    [JsonProperty("baseHolder")] public string BaseHolder { get; set; } = null!;
}

public record PlayerState
{
    [JsonProperty("reputation")] public Dictionary<string, double> Reputation = [];
    [JsonProperty("unlocked")] public Dictionary<string, bool> Unlocked = [];
    [JsonProperty("selectedFaction")] public string? SelectedFaction { get; set; }
}
