using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Routers;
using TerritoryServer.Models;
using TerritoryServer.Servers;

namespace TerritoryServer.Helpers;

[Injectable(InjectionType.Singleton)]
public class ImageRouterHelper(ImageRouter imageRouter, DataConfig dataConfig)
{
    public static readonly string ImagePath = Path.Join(StateServer.ModPath, "Assets", "Images");

    public void LoadFactionImages()
    {
        imageRouter.AddRoute("/files/factions/icon/default_faction", Path.Join(ImagePath, "default_faction.png"));
        
        foreach ((string factionName, Faction faction) in dataConfig.Factions)
        {
            if (faction.Image == null)
                continue;
            
            string path = Path.Join(ImagePath, $"{faction.Image}.png");
            imageRouter.AddRoute($"/files/factions/icon/{faction.Image}", path);
        }
        
    }
}