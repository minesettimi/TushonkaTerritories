using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace TerritoryServer.Models;

public record SaveState
{
    [JsonPropertyName("locations")] public LocationData<LocationState> Locations { get; set; } = new();
    [JsonPropertyName("factions")] public Dictionary<string, FactionState> Factions { get; set; } = [];
    [JsonPropertyName("playerRep")] public Dictionary<MongoId, Dictionary<string, float>> PlayerRep { get; set; } = [];
}

public record LocationState
{
    [JsonPropertyName("holder")] public string Holder { get; set; } = "none";
    [JsonPropertyName("contestants")] public List<string> Contestants { get; set; } = [];
}

public record FactionState
{
    [JsonPropertyName("attitudes")] public Dictionary<string, float> Attitudes = [];
}