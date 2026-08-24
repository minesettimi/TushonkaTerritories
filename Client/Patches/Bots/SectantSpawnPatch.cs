using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.Bots;

public class SectantSpawnPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotZonesLeaveController), nameof(BotZonesLeaveController.DayBlocks));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}