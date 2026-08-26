using System.Reflection;
using EFT;
using EFT.Quests;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.Quests;

namespace TerritoryClient.Patches.Quests;

public class QuestRequirementInitPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(QuestRequirementView), nameof(QuestRequirementView.Init));
    }

    [PatchPostfix]
    public static void Postfix(QuestRequirementView __instance, Condition condition)
    {
        if (condition is ConditionReputation reputationCondition)
        {
            Plugin.PluginLogger.LogInfo("Formatting");
            __instance._text.text = string.Format("UI/Quests/Conditions/Reputation".Localized(),
                $"FactionName {reputationCondition.target}".Localized(), reputationCondition.value.ToString("#0.00"));
            return;
        }
        
        
    }
}