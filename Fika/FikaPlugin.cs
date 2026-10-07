using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using EFT;
using Fika.Core.Main.GameMode;
using Fika.Core.Main.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient;
using TerritoryClient.Services;
using TerritoryClient.Spawns;

namespace Fika
{
    [BepInPlugin("com.minesettimi.territoriesfika", "Tushonka Territories Fika", "1.1.0")]
    [BepInDependency("com.minesettimi.territories", "2.0.0")]
    [BepInDependency("com.fika.core", "2.4.2")]
    public class FikaPlugin : BaseUnityPlugin
    {
        private static PatchManager _patchManager = null!;

        private void Awake()
        {
            _patchManager = new PatchManager(this, true);
            _patchManager.EnablePatches();

            FikaEventDispatcher.SubscribeEvent<FikaGameEndedEvent>(GameEnded);
        }

        private void GameEnded(FikaGameEndedEvent gameEndedEvent)
        {
            if (!gameEndedEvent.IsServer)
                return;
            
            _ = TerritoryPlugin.KillCounter.EndRaid();
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
    
    public class FikaInitializeBotsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HostGameController), nameof(HostGameController.InitializeBotsSystem));
        }

        [PatchPrefix]
        public static void Prefix(AbstractGame ____abstractGame, NonWavesSpawnScenario ____nonWavesSpawnScenario)
        {
            SpawnManager.LocalGameSpawnScenarios.TryGetValue(____abstractGame, out TerritoriesSpawnScenario territoriesSpawnScenario);

            ____nonWavesSpawnScenario.NonWaves =
                ____nonWavesSpawnScenario.NonWaves.AddRangeToArray(territoriesSpawnScenario.BotData);
        }

        [PatchPostfix]
        public static async void Postfix(AbstractGame ____abstractGame, Task __result)
        {
            await __result;
            
            SpawnManager.LocalGameSpawnScenarios.TryGetValue(____abstractGame, out TerritoriesSpawnScenario territoriesSpawnScenario);
        
            territoriesSpawnScenario.Run();
        }
    }
    
    public class FikaStopBotsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HostGameController), nameof(HostGameController.StopBotsSystem));
        }

        [PatchPostfix]
        public static void Postfix(AbstractGame ____abstractGame)
        {
            SpawnManager.LocalGameSpawnScenarios.TryGetValue(____abstractGame, out TerritoriesSpawnScenario territoriesSpawnScenario);
        
            territoriesSpawnScenario.Stop();
        }
    }
}