using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.Spawns;

namespace TerritoryClient.Patches.Spawns;

public class BotStopPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocalGame), nameof(LocalGame.Stop));
    }

    [PatchPrefix]
    public static void Prefix(LocalGame __instance)
    {
        SpawnManager.LocalGameSpawnScenarios.TryGetValue(__instance, out TerritoriesSpawnScenario territoriesSpawnScenario);
        
        territoriesSpawnScenario.Stop();
    }
}