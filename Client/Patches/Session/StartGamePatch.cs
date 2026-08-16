using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.Session;

public class StartGamePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(TarkovApplication), nameof(TarkovApplication.CreateBackend));
    }

    [PatchPostfix]
    public static async void Postfix()
    {
        await Plugin.StateManager.RequestData();
    }
}