using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.Bots;

public class ZoneBlockPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotZonesLeaveController), nameof(BotZonesLeaveController.IsZoneBlockFor));
    }

    [PatchPrefix]
    public static bool Prefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}