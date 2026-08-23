using System.Reflection;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.UI;
using UnityEngine;

namespace TerritoryClient.Patches.UI;

public class MainMenuAwake : ModulePatch
{
    public static ReputationScreen RepScreen;
    public static Tab ReputationTab;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MainMenuShowOperation), nameof(MainMenuShowOperation.Init));
    }

    [PatchPrefix]
    public static void Prefix()
    {
        Transform commonUI = Singleton<CommonUI>.Instance.transform;
        Transform inventoryScreen = commonUI.Find("Common UI/InventoryScreen");
        
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
        
        Transform tabGroup = inventoryScreen.Find("Tab Bar/Tabs");

        GameObject repObj = Object.Instantiate(reputationTab, tabGroup);
        ReputationTab = repObj.GetComponent<Tab>();
        repObj.transform.SetAsLastSibling();
    }
}