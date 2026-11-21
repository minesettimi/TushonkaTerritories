using Newtonsoft.Json;

namespace TerritoryClient.Models;

public class FactionSelectRequest
{
    [JsonProperty("factionName")] public string FactionName { get; set; }
}