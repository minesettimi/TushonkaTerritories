using SPTarkov.DI.Annotations;
using SPTarkov.Server.Web.Models.Configs;
using SPTarkov.Server.Web.Services;
using TerritoryServer.Models;

namespace TerritoryServer.Providers;

[Injectable(InjectionType.Singleton)]
public class TerritoriesEditorProvider(ModConfig modConfig) : IConfigEditorConfigProvider
{
    public IEnumerable<ConfigEditorConfigRegistration> GetConfigs()
    {
        yield return ConfigEditorConfigRegistration.Create(
            "com.minesettimi.territories",
            "Territory Config",
            modConfig,
            Path.Combine("user", "mods", "TushonkaTerritories", "Config", "config.jsonc")
        );
    }
}