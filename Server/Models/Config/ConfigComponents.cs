using System.Text.Json.Serialization;

namespace TerritoryServer.Models;

public record MinMax<T> where T : IComparable<T>
{
    [JsonPropertyName("min")] public required T Min { get; set; }
    [JsonPropertyName("max")] public required T Max { get; set; }
}