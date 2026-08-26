using System.Reflection;
using EFT.Quests;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.Quests;
using UnityEngine;

namespace TerritoryClient.Patches.Quests;

public class QuestIconPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(StaticIcons), nameof(StaticIcons.GetQuestIcon));
    }

    [PatchPrefix]
    public static bool Prefix(StaticIcons __instance, Condition condition, ref Sprite __result)
    {
        if (condition is ConditionReputation)
        {
            __result = __instance.QuestTypeSprites[QuestTemplate.EQuestType.Loyalty];
            return false;
        }

        return true;
    }
}