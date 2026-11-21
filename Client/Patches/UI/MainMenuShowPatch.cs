using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.UI;

public class MainMenuShowPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MenuScreen), nameof(MenuScreen.Show), [typeof(MenuScreen.MainMenuBaseScreenController)]);
    }

    [PatchPostfix]
    public static void Postfix(MenuScreen __instance)
    {
        if (TerritoryPlugin.StateManager.State.SelectedFaction != null)
            return;
        
        __instance.WaitForEndOfFrame(() =>
        {
            ItemUiContext.Instance.RegisterWindow(PreloaderUIAwakePatch.FactionSelectWindow,
                PreloaderUIAwakePatch.FactionSelectWindow.Show());
        });
    }
}