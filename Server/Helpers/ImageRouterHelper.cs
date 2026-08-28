using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Routers;
using TerritoryServer.Models;
using TerritoryServer.Servers;

namespace TerritoryServer.Helpers;

[Injectable(InjectionType.Singleton)]
public class ImageRouterHelper(ImageRouter imageRouter, DataConfig dataConfig)
{
    public static readonly string ImagePath = Path.Join(StateServer.ModPath, "Assets", "Images");
    public static readonly string DefaultImage = Path.Join(ImagePath, "default_faction.png");

    public void LoadFactionImages()
    {
        foreach ((string factionName, Faction faction) in dataConfig.Factions)
        {
            string path = Path.Join(ImagePath, $"{factionName}.png");

            if (!File.Exists(path))
            {
                path = DefaultImage;
            }
            
            imageRouter.AddRoute($"/files/factions/icon/{factionName}", path);
        }
        
    }
}