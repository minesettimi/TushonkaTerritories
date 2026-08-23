using System.Collections.Generic;
using System.Reflection;
using EFT.Communications;
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
        LocationState? locationState = Plugin.StateManager.State.Locations[location.Id];
        
        if (locationState == null)
            return;

        Color factionColor = Plugin.StateManager.ServerData.GetFactionColor(locationState.Holder);
        
        
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