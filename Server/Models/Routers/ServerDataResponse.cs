using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;

namespace TerritoryServer.Models;

public class ServerDataResponse
{
    [JsonPropertyName("factions")] public required Dictionary<string, FactionDataResponse> Factions { get; set; }
    [JsonPropertyName("botFactionTable")] public required Dictionary<string, string> BotFaction { get; set; }
    [JsonPropertyName("spawnSettings")] public required SpawnSettings SpawnSettings { get; set; }
    [JsonPropertyName("attitudeEffect")] public bool AttitudeEffect { get; set; }
    [JsonPropertyName("allyRep")] public double AllyRep { get; set; }
    [JsonPropertyName("neutralRep")] public double NeutralRep { get; set; }
}

public class FactionDataResponse
{
    [JsonPropertyName("color")] public required string FactionColor { get; set; }
    [JsonPropertyName("bots")] public required List<string> FactionBots { get; set; } = [];
    [JsonPropertyName("locked")] public required bool Locked { get; set; }
}

public class ServerStateResponse
{
    [JsonPropertyName("stateId")] public MongoId StateId { get; set; }
    [JsonPropertyName("locations")] public LocationData<LocationState?> Locations { get; set; } = null!;
    [JsonPropertyName("playerState")] public Dictionary<MongoId, PlayerState> PlayerState { get; set; } = null!;
}