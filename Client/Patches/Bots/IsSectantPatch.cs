using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.Bots;

public class IsSectantPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(WildSpawnTypeExtension), nameof(WildSpawnTypeExtension.IsSectant));
    }

    [PatchPrefix]
    public static bool Prefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}