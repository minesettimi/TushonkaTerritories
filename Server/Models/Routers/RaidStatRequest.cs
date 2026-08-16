using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace TerritoryServer.Models;

public class RaidStatRequest : IRequestData
{
    [JsonPropertyName("kills")]
    public Dictionary<string, int> Kills { get; set; } = null!;
    
    [JsonPropertyName("playerKills")]
    public Dictionary<string, Dictionary<string, int>> PlayerKills { get; set; } = null!;
    
    [JsonPropertyName("scav")]
    public bool Scav { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; } = null!;
}