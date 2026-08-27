using System.Reflection;
using EFT.Quests;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.Quests;

public class RewardValuesPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(TaskRewardValuesTextGetter),
            nameof(TaskRewardValuesTextGetter.GetRewardValues));
    }

    [PatchPrefix]
    public static bool Prefix(QuestReward reward, out string typeText, out string nameText, out string valueText,
        out string descriptionText)
    {
        switch (reward.type)
        {
            
        }

        typeText = "";
        nameText = "";
        valueText = "";
        descriptionText = "";

        return true;
    }
}