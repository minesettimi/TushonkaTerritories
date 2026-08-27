using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.Quests;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace TerritoryClient.Patches.Quests;

public class RewardListPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(QuestRewardList), nameof(QuestRewardList.Init));
    }

    [PatchPrefix]
    public static void Prefix(ref IEnumerable<QuestReward> rewards, bool showUnknown, QuestRewardList __instance, out List<QuestReward> __state)
    {
        __state = [];

        List<QuestReward> tempRewards = [.. rewards];
        for(int i = 0; i < tempRewards.Count; i++)
        {
            QuestReward questReward = tempRewards[i];
            
            if (questReward.unknown && !showUnknown)
                continue;
            
            switch (questReward.type)
            {
                case (ERewardType)150:
                {
                    Object.Instantiate(__instance._statPrefab, __instance._container).Show(questReward);
                    tempRewards.RemoveAt(i--);
                    __state.Add(questReward);
                    continue;
                }
                case (ERewardType)151:
                {
                    Object.Instantiate(__instance._traderRewardView, __instance._container).Show(questReward);
                    tempRewards.RemoveAt(i--);
                    __state.Add(questReward);
                    continue;
                }
                default:
                    continue;
            }
        }

        rewards = tempRewards;
    }

    [PatchPostfix]
    public static void Postfix(List<QuestReward> __state, ref IEnumerable<QuestReward> rewards)
    {
        List<QuestReward> tempRewards =
        [
            .. rewards,
            .. __state
        ];

        rewards = tempRewards;
    }
}