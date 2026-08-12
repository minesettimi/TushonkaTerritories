using SPTarkov.Server.Core.Models.Spt.Mod;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace TerritoryServer;

public static class Metadata
{
    public record ModMetadata : IModMetadata
    {
        public string ModGuid { get; init; } = "com.minesettimi.territories";
        public string Name { get; init; } = "Tushonka Territories";
        public string Author { get; init; } = "minesettimi";
        public List<string>? Contributors { get; init; }

        public Version Version { get; init; } = new(0, 1, 0);
        public Range SptVersion { get; init; } = new("~4.1.0");
        
        public bool HasPrepatcher { get; init; }
        public List<string>? Incompatibilities { get; init; }
        public Dictionary<string, Range>? ModDependencies { get; init; }

        public string? Url { get; init; } = "https://github.com/minesettimi/TushonkaTerritories";
        public string License { get; init; } = "MIT";
    }
}