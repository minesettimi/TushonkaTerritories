using System.Reflection;
using EFT.Communications;
using EFT.UI.Matchmaker;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.UI;
using UnityEngine;

namespace TerritoryClient.Patches.UI;

public class LocationSelectionAwakePatch : ModulePatch
{
    public static TerritoryRenderer TerritoryRenderer;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MatchMakerSelectionLocationScreen),
            nameof(MatchMakerSelectionLocationScreen.Awake));
    }

    [PatchPostfix]
    public static void Postfix(MatchMakerSelectionLocationScreen __instance)
    {
        GameObject? territoryMap = Plugin.BundleLoader.Bundle.LoadAsset<GameObject>("TerritoryMap.prefab");

        if (territoryMap == null)
        {
            NotificationManager.DisplayMessageNotification("Error loading territory map from bundle.");
            return;
        }

        Transform map = __instance.transform.Find("Content/Map");
        Transform mapImage = map.transform.Find("Image");

        GameObject territoryObj = Object.Instantiate(territoryMap, mapImage);
        territoryObj.transform.SetAsFirstSibling();
        
        TerritoryRenderer = territoryObj.GetComponent<TerritoryRenderer>();
        TerritoryRenderer.MapTransform = map.gameObject.GetComponent<RectTransform>();
    }
}