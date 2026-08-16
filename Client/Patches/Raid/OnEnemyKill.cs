using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.Raid;

public class OnEnemyKill : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BaseStatisticsManager), nameof(BaseStatisticsManager.OnEnemyKill));
    }

    [PatchPostfix]
    public static void Postfix(WildSpawnType role)
    {
        Plugin.KillCounter.KilledEnemy(role.ToString());
    }
}