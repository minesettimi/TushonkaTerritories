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
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(InventoryScreen), nameof(InventoryScreen.Show), [typeof(InventoryScreen.InventoryScreenController)]);
    }

    [PatchPostfix]
    public static void Postfix(InventoryScreen __instance, IEftSession ____backEndSession,
        InventoryController ____inventoryController, InventoryScreen.InventoryScreenController ___ScreenController)
    {
        ReputationScreen reputationTab = CommonUIAwakePatch.RepScreen;
        
        Tab repTab = __instance._tabDictionary[CommonUIAwakePatch.RepEnumValue];
        repTab.Init(new ReputationScreen.ReputationTabController(reputationTab, ____backEndSession.AllProfiles, ____inventoryController));
        repTab.SetInteractable(___ScreenController is { InRaid: false, IsInventoryBlocked: false });
    }
}