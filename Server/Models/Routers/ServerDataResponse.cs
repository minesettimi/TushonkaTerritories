using System.Text.Json.Serialization;

namespace TerritoryServer.Models;

public class ServerDataResponse
{
    [JsonPropertyName("factionColors")] public Dictionary<string, string> FactionColors { get; set; } = null!;
    [JsonPropertyName("botFactionTable")] public Dictionary<string, string> BotFaction { get; set; } = [];
    [JsonPropertyName("attitudeEffect")] public bool AttitudeEffect { get; set; }
    [JsonPropertyName("allyRep")] public double AllyRep { get; set; }
    [JsonPropertyName("neutralRep")] public double NeutralRep { get; set; }
    [JsonPropertyName("continualUpdates")] public bool ContinualUpdates { get; set; }
}