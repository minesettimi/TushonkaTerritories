using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.Communications;

public class InitNotificationManagerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(TarkovApplication), nameof(TarkovApplication.InitNotificationManager));
    }

    [PatchPostfix]
    public static void Postfix()
    {
        Plugin.StateManager.Init();
    }
}