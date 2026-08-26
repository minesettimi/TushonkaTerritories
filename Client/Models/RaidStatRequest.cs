using System.Collections.Generic;
using Newtonsoft.Json;

namespace TerritoryClient.Models;

public class RaidStatRequest
{
    [JsonProperty("kills")]
    public Dictionary<string, int> Kills { get; set; } = null!;
    
    [JsonProperty("playerKills")]
    public Dictionary<string, Dictionary<string, int>> PlayerKills { get; set; } = null!;

    [JsonProperty("location")]
    public string Location { get; set; } = null!;
}