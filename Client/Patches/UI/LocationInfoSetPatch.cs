using System.Reflection;
using EFT;
using EFT.UI;
using HarmonyLib;
using JsonType;
using SPT.Reflection.Patching;
using TerritoryClient.Models;
using TMPro;

namespace TerritoryClient.Patches.UI;

public class LocationInfoSetPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocationInfoPanel), nameof(LocationInfoPanel.Set));
    }

    [PatchPrefix]
    public static void Set(LocationInfoPanel __instance, LocationSettings.Location? location, IEftSession session)
    {
        if (location == null)
            return;
        
        LocationState? locationState = Plugin.StateManager.State.Locations[location.Id];
        
        if (locationState == null)
            return;

        TextMeshProUGUI description = LocationSelectionAwakePatch.TerritoryDescription;
        
        description.text = string.Format("TerritoryDescription".Localized(), $"FactionName {locationState.Holder}".Localized());
    }
}