using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace TerritoryServer.Models;

public class FactionSelectRequest : IRequestData
{
    [JsonPropertyName("factionName")] public string FactionName { get; set; } = "none";
}