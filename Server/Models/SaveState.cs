using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace TerritoryServer.Models;

public record SaveState
{
    [JsonPropertyName("locations")] public LocationData<LocationState> Locations { get; set; } = new();
    [JsonPropertyName("playerRep")] public Dictionary<MongoId, Dictionary<string, double>> PlayerRep { get; set; } = [];
}

public record LocationState
{
    [JsonPropertyName("holder")] public string Holder { get; set; } = "none";
    [JsonPropertyName("contestants")] public Dictionary<string, double> Contestants { get; set; } = []; //faction to strength
    [JsonPropertyName("base")] public bool Base { get; set; } = false;
}