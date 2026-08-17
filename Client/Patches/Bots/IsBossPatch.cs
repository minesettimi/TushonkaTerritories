using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.Bots;

public class IsBossPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(WildSpawnTypeExtension), nameof(WildSpawnTypeExtension.IsBoss));
    }

    [PatchPrefix]
    public static bool Prefix(ref bool __result)
    {
        //__result = true;
        //return false;
        return true;
    }
}