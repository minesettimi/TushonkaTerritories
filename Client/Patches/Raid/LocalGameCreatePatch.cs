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
        Profile currentProfile = __instance._raidSettings.Side == ESideType.Pmc 
            ? __instance.Session.Profile : __instance.Session.ProfileOfPet; //wtf BSG
        
        Plugin.KillCounter.StartRaid(currentProfile.Id, __instance._raidSettings.LocationId);
    }
}