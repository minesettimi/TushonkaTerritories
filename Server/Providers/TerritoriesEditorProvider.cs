using SPTarkov.DI.Annotations;
using SPTarkov.Server.Web.Models.Configs;
using SPTarkov.Server.Web.Services;
using TerritoryServer.Models;
using TerritoryServer.Servers;

namespace TerritoryServer.Providers;

[Injectable(InjectionType.Singleton)]
public class TerritoriesEditorProvider(ModConfig modConfig,
    StateServer stateServer) : IConfigEditorConfigProvider
{
    public IEnumerable<ConfigEditorConfigRegistration> GetConfigs()
    {
        yield return new ConfigEditorConfigRegistration
        {
            Id = "com.minesettimi.territories",
            DisplayName = "Territory Config",
            RuntimeConfig = modConfig,
            RuntimeType = typeof(ModConfig),
            FilePath = Path.Combine("user", "mods", "TushonkaTerritories", "Config", "config.jsonc"),
            OnAppliedToRuntimeAsync = (_, _) =>
            {
                stateServer.SendStateUpdate();
                return new ValueTask();
            }
        };
    }
}