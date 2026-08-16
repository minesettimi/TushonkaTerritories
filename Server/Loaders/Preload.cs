using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using TerritoryServer.Generators;
using TerritoryServer.Servers;

namespace TerritoryServer.Loaders;

[Injectable(TypePriority = OnLoadOrder.Preload + 40)]
public class Preload(StateServer stateServer,
    StateGenerator stateGenerator,
    BotConfig botConfig) : IOnLoad
{
    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        await stateServer.LoadSave();

        if (stateServer.NewSave)
        {
            stateServer.CurrentSave = stateGenerator.GenerateState();
            stateServer.SaveToDisk();
        }
        
        botConfig.WeeklyBoss.Enabled = false;
    }
}