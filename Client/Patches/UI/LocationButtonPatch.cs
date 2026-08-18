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
        
        string color = Plugin.StateManager.ServerData.FactionColors[locationState.Holder];
        
        if (!ColorUtility.TryParseHtmlString(color, out Color colorObj))
        {
            NotificationManager.DisplayMessageNotification($"Failed to parse color {color}!");
            return;
        }
        
        __instance._defaultColor = colorObj;
        __instance._specialColor = colorObj;
    }

    [PatchPostfix]
    public static void Postfix(LocationSettings.Location location, LocationButton __instance)
    {
        TerritoryRenderer.LocationPositions[location.Id.ToLower()] = __instance.transform.position;
    }
}