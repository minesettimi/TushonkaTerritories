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
    public static void Prefix(LocationInfoPanel __instance, LocationSettings.Location? location, IEftSession session)
    {
        if (location == null)
            return;
        
        LocationState? locationState = TerritoryPlugin.StateManager.State.Locations[location.Id];
        
        if (locationState == null)
            return;

        TextMeshProUGUI controlledDesc = LocationSelectionAwakePatch.TerritoryControlled;
        TextMeshProUGUI contestedDesc = LocationSelectionAwakePatch.TerritoryContested;

        controlledDesc.text = string.Format("TerritoryControlled".Localized(),
            $"FactionName {locationState.Holder}".Localized() +
            $" ({locationState.Contestants[locationState.Holder]:0.0#})");

        if (locationState.Contestants.Count < 2)
        {
            contestedDesc.gameObject.SetActive(false);
            return;
        }

        contestedDesc.gameObject.SetActive(true);
        
        //form list of translated strings
        List<string> factionNames = [];
        
        foreach ((string faction, double strength) in locationState.Contestants)
        {
            if (faction == locationState.Holder)
                continue;

            factionNames.Add($"FactionName {faction}".Localized() + $" ({strength:0.0#})");
        }
        
        contestedDesc.text = string.Format("TerritoryContested".Localized(), string.Join(", ", factionNames));
    }
}