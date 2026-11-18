using System.Text.Json.Serialization;
using TerritoryServer.Services;

namespace TerritoryServer.Models;

public record MinMax<T> where T : IComparable<T>
{
    [JsonPropertyName("min")] public required T Min { get; set; }
    [JsonPropertyName("max")] public required T Max { get; set; }
}

public class LocationData<T> : Dictionary<string, T>
{
    public T? this[string key]
    {
        get
        {
            string adjustedKey = key;
            if (LocationService.DuplicateMapIndex.TryGetValue(adjustedKey, out string? newKey))
            {
                adjustedKey = newKey;
            }

            return this.GetValueOrDefault(adjustedKey);
        }
        set
        {
            string adjustedKey = key;
            if (LocationService.DuplicateMapIndex.TryGetValue(adjustedKey, out string? newKey))
            {
                adjustedKey = newKey;
            }
            
            base[adjustedKey] = value!;
        }
    }
}

