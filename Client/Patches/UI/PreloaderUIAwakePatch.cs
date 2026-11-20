using System.Reflection;
using EFT.Communications;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.UI;
using UnityEngine;

namespace TerritoryClient.Patches.UI;

public class PreloaderUIAwakePatch : ModulePatch
{
    public static FactionSelectWindow FactionSelectWindow = null!;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(PreloaderUI), nameof(PreloaderUI.Awake));
    }

    [PatchPrefix]
    public static void PreloaderUIAwake(PreloaderUI __instance)
    {
        Transform uiContext = __instance._UIContext.transform;
        
        GameObject? factionSelect = TerritoryPlugin.BundleLoader.Bundle.LoadAsset<GameObject>("FactionChoiceWindow.prefab");
        
        if (factionSelect == null)
        {
            TerritoryPlugin.PluginLogger.LogError("Failed to load bundle.");
            NotificationManager.DisplayMessageNotification("Error loading Reputation tab button from bundle.");
            return;
        }
        
        Transform windows = uiContext.transform.Find("WindowsPlaceholder");

        //fix serialization field that requires Odin to work
        Transform button = factionSelect.transform.Find("Inner/ConfirmButton");
        TweenAnimatedButton tweenButton = button.GetComponent<TweenAnimatedButton>();
        tweenButton._animation = button.GetComponent<DefaultUIButtonAnimation>();
        
        GameObject? factionChoiceWindow = Object.Instantiate(factionSelect, windows);

        if (factionChoiceWindow == null)
        {
            return;
        }

        FactionSelectWindow = factionChoiceWindow.GetComponent<FactionSelectWindow>();
        factionChoiceWindow.SetActive(false);
    }
}