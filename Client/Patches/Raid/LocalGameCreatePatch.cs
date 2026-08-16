using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.Raid;

public class LocalGameCreatePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(TarkovApplication), nameof(TarkovApplication.LocalGameCreate));
    }

    [PatchPrefix]
    public static void Prefix(TarkovApplication __instance)
    {
        Plugin.KillCounter.StartRaid(__instance._raidSettings.Side != ESideType.Pmc,
            __instance._raidSettings.LocationId);
    }
}