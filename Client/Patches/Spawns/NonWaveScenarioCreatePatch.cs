using System.Reflection;
using EFT;
using HarmonyLib;
using JsonType;
using SPT.Reflection.Patching;
using TerritoryClient.Spawns;

namespace TerritoryClient.Patches.Spawns;

public class NonWaveScenarioCreatePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(NonWavesSpawnScenario), nameof(NonWavesSpawnScenario.Create));
    }

    [PatchPostfix]
    public static void Postfix(AbstractGame game, LocationSettings.Location location, BotsController botsController)
    {
        SpawnManager.LocalGameSpawnScenarios.AddOrUpdate(game, TerritoriesSpawnScenario.Create(game, location, botsController));
    }
}