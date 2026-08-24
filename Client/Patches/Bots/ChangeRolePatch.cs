using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.Bots;

public class ChangeRolePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ProfileSettings), nameof(ProfileSettings.TryChangeRoleToAssaultGroup));
    }

    [PatchPrefix]
    public static bool Prefix(ProfileSettings __instance)
    {
        if (__instance.Role == WildSpawnType.assaultGroup)
        {
            __instance.Role = WildSpawnType.assault;
        }
        
        return false;
    }
}