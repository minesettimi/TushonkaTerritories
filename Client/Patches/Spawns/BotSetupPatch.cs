using System.Reflection;
using System.Threading.Tasks;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.Spawns;

namespace TerritoryClient.Patches.Spawns;

public class BotSetupPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocalGame), nameof(LocalGame.vmethod_1));
    }

    [PatchPrefix]
    public static void Prefix(LocalGame __instance, NonWavesSpawnScenario ____nonWavesSpawnScenario)
    {
        SpawnManager.LocalGameSpawnScenarios.TryGetValue(__instance, out TerritoriesSpawnScenario territoriesSpawnScenario);

        ____nonWavesSpawnScenario.NonWaves =
            ____nonWavesSpawnScenario.NonWaves.AddRangeToArray(territoriesSpawnScenario.BotData);
    }

    [PatchPostfix]
    public static async void Postfix(LocalGame __instance, Task __result)
    {
        await __result;
        
        SpawnManager.LocalGameSpawnScenarios.TryGetValue(__instance, out TerritoriesSpawnScenario territoriesSpawnScenario);
        
        territoriesSpawnScenario.Run();
    }
}