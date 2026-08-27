using System.Reflection;
using EFT.Communications;
using EFT.UI.Matchmaker;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.UI;
using TMPro;
using UnityEngine;

namespace TerritoryClient.Patches.UI;

public class LocationSelectionAwakePatch : ModulePatch
{
    public static TerritoryRenderer TerritoryRenderer;
    public static TextMeshProUGUI TerritoryControlled;
    public static TextMeshProUGUI TerritoryContested;
    
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
            Plugin.PluginLogger.LogError("Failed to load bundle.");
            NotificationManager.DisplayMessageNotification("Error loading territory map from bundle.");
            return;
        }

        Transform map = __instance.transform.Find("Content/Map");
        Transform mapImage = map.transform.Find("Image");

        GameObject territoryObj = Object.Instantiate(territoryMap, mapImage);
        territoryObj.transform.SetAsFirstSibling();
        
        TerritoryRenderer = territoryObj.GetComponent<TerritoryRenderer>();
        TerritoryRenderer.MapTransform = map.gameObject.GetComponent<RectTransform>();

        GameObject? descriptionAsset = Plugin.BundleLoader.Bundle.LoadAsset<GameObject>("TerritoryInfo.prefab");

        if (descriptionAsset == null)
        {
            Plugin.PluginLogger.LogError("Failed to load bundle.");
            NotificationManager.DisplayMessageNotification("Error loading territory description from bundle.");
            return;
        }

        Transform locationInfoPanel = __instance._infoPanel.transform.Find("DescriptionPanel");
        GameObject descriptionObj = Object.Instantiate(descriptionAsset, locationInfoPanel);
        descriptionObj.name = "TerritoryDescription";
        descriptionObj.transform.SetAsFirstSibling();

        Transform controlled = descriptionObj.transform.Find("ControlledLabel");
        Transform contested = descriptionObj.transform.Find("ContestedLabel");
        
        TerritoryControlled = controlled.GetComponent<TextMeshProUGUI>();
        TerritoryContested = contested.GetComponent<TextMeshProUGUI>();
    }
}