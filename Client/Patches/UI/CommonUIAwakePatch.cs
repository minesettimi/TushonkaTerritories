using System;
using System.Collections.Generic;
using System.Reflection;
using EFT.Communications;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TerritoryClient.Patches.UI;

public class CommonUIAwakePatch : ModulePatch
{
    public static ReputationScreen RepScreen;
    public static Tab ReputationTab;
    public static AnimatedToggle TemplateToggle;
    
    public static EInventoryTab RepEnumValue;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(CommonUI), nameof(CommonUI.Awake));
    }

    [PatchPostfix]
    public static void Postfix(CommonUI __instance)
    {
        //instantiate everything
        Transform inventoryTransform = __instance.InventoryScreen.transform;
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

        GameObject repScreenObj = Object.Instantiate(reputationScreen, inventoryTransform);
        RepScreen = repScreenObj.GetComponent<ReputationScreen>();
        repScreenObj.name = "Reputation Panel";
        
        Transform tabGroup = inventoryTransform.Find("Tab Bar/Tabs");

        GameObject repObj = Object.Instantiate(reputationTab, tabGroup);
        repObj.name = "Reputation";
        ReputationTab = repObj.GetComponent<Tab>();
        repObj.transform.SetAsLastSibling();
        
        
        //steal the prefab
        Transform overallSpawner =
            inventoryTransform.Find("Overall Panel/RightSide/Buttons/Placeholder/Overall/OverallToggleSpawner");

        UIAnimatedToggleSpawner toggleSpawner = overallSpawner.gameObject.GetComponent<UIAnimatedToggleSpawner>();
        TemplateToggle = toggleSpawner._object;
        
        InventoryScreen inventoryScreen = __instance.InventoryScreen;
        
        
        //get enum and add to tab dictionary
        if (!Enum.TryParse("Reputation", out EInventoryTab repEnum))
        {
            Plugin.PluginLogger.LogError("Failed to get Reputation enum value!");
            return;
        }

        RepEnumValue = repEnum;
        
        IReadOnlyDictionary<EInventoryTab, Tab> tabDictionary = inventoryScreen._tabDictionary;

        if (tabDictionary.ContainsKey(RepEnumValue))
            return;
        
        Dictionary<EInventoryTab, Tab> fixedDictionary = [];

        foreach ((EInventoryTab enumTab, Tab tab) in tabDictionary)
        {
            fixedDictionary.Add(enumTab, tab);
        }
        
        fixedDictionary.Add(RepEnumValue, ReputationTab);
        inventoryScreen._tabDictionary = fixedDictionary;
    }
}