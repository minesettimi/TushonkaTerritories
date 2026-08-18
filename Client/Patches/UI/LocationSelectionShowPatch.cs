using System.Reflection;
using EFT;
using EFT.UI.Matchmaker;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.UI;

public class LocationSelectionShowPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MatchMakerSelectionLocationScreen),
            nameof(MatchMakerSelectionLocationScreen.Show), 
            [typeof(IEftSession), typeof(RaidSettings), typeof(MatchmakerPlayersController)]);
    }

    [PatchPostfix]
    public static void Postfix(IEftSession session)
    {
        LocationSelectionAwakePatch.TerritoryRenderer.Show(session.LocationSettings);
        Plugin.PluginLogger.LogInfo("Test Show.");
    }
}