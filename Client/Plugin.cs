using System;
using BepInEx;
using BepInEx.Logging;
using SPT.Reflection.Patching;
using TerritoryClient.Services;

namespace TerritoryClient
{
    [BepInPlugin("com.minesettimi.territories", "Tushonka Territories", "0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource PluginLogger;
        public static KillCounter KillCounter;
        public static StateManager StateManager;
        
        private PatchManager _patchManager;

        protected void Awake()
        {
            PluginLogger = Logger;
            
            _patchManager = new PatchManager(this, true);
            _patchManager.EnablePatches();

            KillCounter = new KillCounter();
            StateManager = new StateManager();
        }
    }
}