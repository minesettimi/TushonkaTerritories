using System.Reflection;
using EFT.Communications;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.UI;
using UnityEngine;

namespace TerritoryClient.Patches.UI;

public class CommonUIAwakePatch : ModulePatch
{
    public static ReputationScreen RepScreen;
    public static Tab ReputationTab;
    public static AnimatedToggle TemplateToggle;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(CommonUI), nameof(CommonUI.Awake));
    }

    [PatchPostfix]
    public static void Postfix(CommonUI __instance)
    {
        Transform inventoryScreen = __instance.InventoryScreen.transform;
        
        GameObject? reputationTab = Plugin.BundleLoader.Bundle.LoadAsset<GameObject>("Reputation.prefab");

        if (reputationTab == null)
        {
            Plugin.PluginLogger.LogError("Failed to load bundle.");
            NotificationManager.DisplayMessageNotification("Error loading Reputation tab button from bundle.");
            return;
        }
        
        GameObject? reputationScreen = Plugin.BundleLoader.Bundle.LoadAsset<GameObject>("ReputationPanel.prefab");

        if (reputationTab == null)
        {
            Plugin.PluginLogger.LogError("Failed to load bundle.");
            NotificationManager.DisplayMessageNotification("Error loading Reputation screen from bundle.");
            return;
        }

        GameObject repScreenObj = Object.Instantiate(reputationScreen, inventoryScreen);
        RepScreen = repScreenObj.GetComponent<ReputationScreen>();
        repScreenObj.name = "Reputation Panel";
        
        Transform tabGroup = inventoryScreen.Find("Tab Bar/Tabs");

        GameObject repObj = Object.Instantiate(reputationTab, tabGroup);
        repObj.name = "Reputation";
        ReputationTab = repObj.GetComponent<Tab>();
        repObj.transform.SetAsLastSibling();
        
        
        //steal the prefab
        Transform overallSpawner =
            inventoryScreen.Find("Overall Panel/RightSide/Buttons/Placeholder/Overall/OverallToggleSpawner");

        UIAnimatedToggleSpawner toggleSpawner = overallSpawner.gameObject.GetComponent<UIAnimatedToggleSpawner>();
        TemplateToggle = toggleSpawner._object;
    }
}