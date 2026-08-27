using System.Collections.Generic;
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

        TextMeshProUGUI controlledDesc = LocationSelectionAwakePatch.TerritoryControlled;
        TextMeshProUGUI contestedDesc = LocationSelectionAwakePatch.TerritoryContested;
        
        controlledDesc.text = string.Format("TerritoryControlled".Localized(), $"FactionName {locationState.Holder}".Localized());

        if (locationState.Contestants.Count < 2)
        {
            contestedDesc.gameObject.SetActive(false);
            return;
        }

        contestedDesc.gameObject.SetActive(true);
        
        //form list of translated strings
        List<string> factionNames = [];
        
        foreach (string faction in locationState.Contestants.Keys)
        {
            if (faction == locationState.Holder)
                continue;

            factionNames.Add($"FactionName {faction}".Localized());
        }
        
        contestedDesc.text = string.Format("TerritoryContested".Localized(), string.Join(", ", factionNames));
    }
}