using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common;

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
    [JsonPropertyName("color")] public required string FactionColor { get; set; }
    [JsonPropertyName("bots")] public required WildSpawnType[] FactionBots { get; set; } = [];
    [JsonPropertyName("locked")] public required bool Locked { get; set; }
}