using System.Reflection;
using EFT.UI;
using HarmonyLib;
using JsonType;
using SPT.Reflection.Patching;
using TerritoryClient.Models;
using TerritoryClient.UI;
using UnityEngine;

namespace TerritoryClient.Patches.UI;

public class LocationButtonPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocationButton), nameof(LocationButton.Show));
    }

    [PatchPrefix]
    public static void Prefix(LocationSettings.Location location, LocationButton __instance)
    {
        LocationState? locationState = TerritoryPlugin.StateManager.State.Locations[location.Id];
        
        if (locationState == null)
            return;

        if (!TerritoryPlugin.StateManager.ServerData.Factions.TryGetValue(locationState.Holder, out FactionData? faction))
        {
            TerritoryPlugin.PluginLogger.LogError($"Failed to get color for faction: {locationState.Holder}");
            return;
        }
        Color factionColor = faction.Color;
        
        __instance._defaultColor = factionColor;
        __instance._specialColor = factionColor;
    }

    [PatchPostfix]
    public static void Postfix(LocationSettings.Location location, LocationButton __instance)
    {
        RectTransform rectTransform = (RectTransform)__instance.transform;

        TerritoryRenderer.LocationPositions[location.Id.ToLower()] =
            RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);
    }
}