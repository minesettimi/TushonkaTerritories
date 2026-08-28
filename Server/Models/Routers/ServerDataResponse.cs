using System.Text.Json.Serialization;

namespace TerritoryServer.Models;

public class ServerDataResponse
{
    [JsonPropertyName("factions")] public Dictionary<string, FactionDataResponse> Factions { get; set; } = null!;
    [JsonPropertyName("botFactionTable")] public Dictionary<string, string> BotFaction { get; set; } = [];
    [JsonPropertyName("attitudeEffect")] public bool AttitudeEffect { get; set; }
    [JsonPropertyName("allyRep")] public double AllyRep { get; set; }
    [JsonPropertyName("neutralRep")] public double NeutralRep { get; set; }
}

public class FactionDataResponse
{
    [JsonPropertyName("color")] public string FactionColor { get; set; } = null!;
    [JsonPropertyName("locked")] public bool Locked { get; set; }
}