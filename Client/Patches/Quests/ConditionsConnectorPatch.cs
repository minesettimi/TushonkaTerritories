using System;
using System.Reflection;
using EFT.Quests;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.Quests;

namespace TerritoryClient.Patches.Quests;

public class ConditionsConnectorPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ConditionsConnectorsManager<IConditional>),
            nameof(ConditionsConnectorsManager<>.InvokeConditionsConnector));
    }

    [PatchPrefix]
    public static bool Prefix(ConditionsConnectorsManager<IConditional> __instance, IConditional conditional, Condition condition, EQuestStatus status)
    {
        if (condition is ConditionReputation reputationCondition)
        {
            ConditionProgressChecker progressChecker = conditional.ProgressCheckers[reputationCondition];
            progressChecker.SetCurrentValueGetter(_ => Plugin.StateManager.State.GetPlayerRep(__instance.Profile.Id, reputationCondition.target));
            
            Action repUpdated = () => __instance.OnConditionValueChanged?.Invoke(conditional, status, reputationCondition, true);

            Plugin.StateManager.StateUpdated += repUpdated;
            progressChecker.OnDisconnect += _ => Plugin.StateManager.StateUpdated -= repUpdated;
            progressChecker.OnReset += _ => __instance.ForceReset(conditional, status, condition);
            
            return false;
        }

        return true;
    }
}