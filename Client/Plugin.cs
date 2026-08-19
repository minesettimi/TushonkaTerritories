using BepInEx;
using BepInEx.Logging;
using SPT.Reflection.Patching;
using TerritoryClient.Bundles;
using TerritoryClient.Services;

namespace TerritoryClient
{
    [BepInPlugin("com.minesettimi.territories", "Tushonka Territories", "1.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource PluginLogger;
        public static KillCounter KillCounter;
        public static StateManager StateManager;
        public static BundleLoader BundleLoader;
        
        private PatchManager _patchManager;

        protected void Awake()
        {
            PluginLogger = Logger;
            
            _patchManager = new PatchManager(this, true);
            _patchManager.EnablePatches();

            KillCounter = new KillCounter();
            StateManager = new StateManager();
            BundleLoader = new BundleLoader();
        }
    }
}