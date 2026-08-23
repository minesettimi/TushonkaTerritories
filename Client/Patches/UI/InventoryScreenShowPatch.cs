using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.UI;

namespace TerritoryClient.Patches.UI;

public class InventoryScreenShowPatch : ModulePatch
{
    public static EInventoryTab RepEnumValue;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(InventoryScreen), nameof(InventoryScreen.Show), [typeof(InventoryScreen.InventoryScreenController)]);
    }

    [PatchPrefix]
    public static void Prefix(InventoryScreen __instance)
    {
        if (!Enum.TryParse("Reputation", out EInventoryTab repEnum))
        {
            Plugin.PluginLogger.LogError("Failed to get Reputation enum value!");
            return;
        }

        RepEnumValue = repEnum;
        
        //add new entry to tab dictionary
        IReadOnlyDictionary<EInventoryTab, Tab> tabDictionary = __instance._tabDictionary;
        Dictionary<EInventoryTab, Tab> fixedDictionary = [];

        foreach ((EInventoryTab enumTab, Tab tab) in tabDictionary)
        {
            fixedDictionary.Add(enumTab, tab);
        }
        
        fixedDictionary.Add(RepEnumValue, CommonUIAwakePatch.ReputationTab);
        __instance._tabDictionary = fixedDictionary;
    }

    [PatchPostfix]
    public static void Postfix(InventoryScreen __instance, IEftSession ____backEndSession,
        InventoryController ____inventoryController, InventoryScreen.InventoryScreenController ___ScreenController)
    {
        ReputationScreen reputationTab = CommonUIAwakePatch.RepScreen;
        
        Tab repTab = __instance._tabDictionary[RepEnumValue];
        repTab.Init(new ReputationScreen.ReputationTabController(reputationTab, ____backEndSession.AllProfiles, ____inventoryController));
        repTab.SetInteractable(___ScreenController is { InRaid: false, IsInventoryBlocked: false });
    }
}