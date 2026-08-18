using System;
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
    public static void Postfix()
    {
        try
        {
            LocationSelectionAwakePatch.TerritoryRenderer.Show();
        }
        catch (Exception e)
        {
            Plugin.PluginLogger.LogError($"Failed to render map with error: {e.Message}");
            Plugin.PluginLogger.LogError(e.StackTrace);
            throw;
        }
    }
}