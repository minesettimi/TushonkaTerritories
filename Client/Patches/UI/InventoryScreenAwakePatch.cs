using System;
using System.Collections.Generic;
using System.Reflection;
using EFT.Communications;
using EFT.UI;
using HarmonyLib;
using Newtonsoft.Json;
using SPT.Reflection.Patching;
using TerritoryClient.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TerritoryClient.Patches.UI;

public class InventoryScreenAwakePatch : ModulePatch
{
    public static EInventoryTab RepEnumValue;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(InventoryScreen), nameof(InventoryScreen.Awake));
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
        
        fixedDictionary.Add(RepEnumValue, MainMenuAwake.ReputationTab);
        __instance._tabDictionary = fixedDictionary;
    }
}