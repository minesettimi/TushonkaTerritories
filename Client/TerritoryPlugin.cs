using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using SPT.Reflection.Patching;
using TerritoryClient.Bundles;
using TerritoryClient.Services;

namespace TerritoryClient
{
    [BepInPlugin("com.minesettimi.territories", "Tushonka Territories", "1.3.4")]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    public class TerritoryPlugin : BaseUnityPlugin
    {
        public static ManualLogSource PluginLogger = null!;
        public static KillCounter KillCounter = null!;
        public static StateManager StateManager = null!;
        public static BundleLoader BundleLoader = null!;
        
        private PatchManager _patchManager = null!;

        public static bool IsFika;
        public static bool IsSyncPresent;

        protected void Awake()
        {
            PluginLogger = Logger;
            
            _patchManager = new PatchManager(this, true);
            _patchManager.EnablePatches();

            KillCounter = new KillCounter();
            StateManager = new StateManager();
            BundleLoader = new BundleLoader();

            IsFika = Chainloader.PluginInfos.Keys.Contains("com.fika.core");
        }
    }
}