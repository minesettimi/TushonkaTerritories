using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace TerritoryClient.Bundles;

//basic crappy bundle loader to load one bundle
public class BundleLoader
{
    public static readonly string ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    public static readonly string BundlePath = Path.Combine(ModPath, "Bundles", "ttbundle.bundle");

    public AssetBundle Bundle;
    
    public BundleLoader()
    {
        Task.Run(LoadBundle);
    }
    
    public async Task LoadBundle()
    {
        AssetBundleCreateRequest? bundle = AssetBundle.LoadFromFileAsync(BundlePath);

        while (!bundle.isDone)
        {
            await Task.Yield();
        }
        
        Bundle = bundle.assetBundle;
    }
}