using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using TerritoryServer.Generators;
using TerritoryServer.Servers;
using TerritoryServer.Services;

namespace TerritoryServer.Loaders;

[Injectable(TypePriority = OnLoadOrder.Preload + 40)]
public class Preload(StateServer stateServer,
    StateGenerator stateGenerator,
    LocationConfig locationConfig,
    PmcConfig pmcConfig,
    BotConfig botConfig,
    CacheService cacheService,
    IEnumerable<IRuntimePatch> patches) : IOnLoad
{
    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        foreach (IRuntimePatch patch in patches)
        {
            patch.Enable();
        }
        
        cacheService.Initialize();
        await stateServer.LoadSave();

        if (stateServer.NewSave)
        {
            stateServer.CurrentSave = stateGenerator.GenerateState();
            stateServer.SaveToDisk();
        }
        
        ChangeVanillaSettings();
    }

    private void ChangeVanillaSettings()
    {
        locationConfig.AddCustomBotWavesToMaps = false;
        locationConfig.EnableBotTypeLimits = false;
        locationConfig.AddOpenZonesToAllMaps = false;
        locationConfig.RogueLighthouseSpawnTimeSettings.Enabled = false;
        botConfig.WeeklyBoss.Enabled = false;
        pmcConfig.CustomPmcWaves.Clear();
    }
}