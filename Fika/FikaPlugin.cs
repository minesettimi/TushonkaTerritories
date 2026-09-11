using System;
using System.Reflection;
using BepInEx;
using Fika.Core.Main.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient;
using TerritoryClient.Patches.Raid;
using TerritoryClient.Services;

namespace Fika
{
    [BepInPlugin("com.minesettimi.territoriesfika", "Tushonka Territories Fika", "1.0.2")]
    [BepInDependency("com.minesettimi.territories", "1.3.3")]
    [BepInDependency("com.fika.core", "2.4.2")]
    public class FikaPlugin : BaseUnityPlugin
    {
        private static PatchManager _patchManager = null!;

        private void Awake()
        {
            _patchManager = new PatchManager(this, true);
            _patchManager.EnablePatches();

            TerritoryPlugin.IsSyncPresent = true;
            FikaEventDispatcher.SubscribeEvent<FikaGameEndedEvent>(GameEnded);
        }

        private void GameEnded(FikaGameEndedEvent gameEndedEvent)
        {
            _ = TerritoryPlugin.KillCounter.EndRaid(gameEndedEvent.IsServer);
        }
    }
    
    public class KillCounterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(KillCounter), nameof(KillCounter.StartRaid));
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return FikaBackendUtils.IsServer;
        }
    }
}