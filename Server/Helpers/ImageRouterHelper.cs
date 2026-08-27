using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Routers;
using TerritoryServer.Servers;

namespace TerritoryServer.Helpers;

[Injectable(InjectionType.Singleton)]
public class ImageRouterHelper(ImageRouter imageRouter)
{
    public static readonly string ImagePath = Path.Join(StateServer.ModPath, "Assets", "Images");

    public void LoadFactionImages()
    {
        if (!Directory.Exists(ImagePath))
            return;
        
        IEnumerable<string> factionImages = Directory.EnumerateFiles(ImagePath, "*", SearchOption.TopDirectoryOnly);
        foreach (string path in factionImages)
        {
            string imageName = Path.GetFileNameWithoutExtension(path);
            imageRouter.AddRoute($"/files/factions/icon/{imageName}", path);
        }
    }
}