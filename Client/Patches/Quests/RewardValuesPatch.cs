using System.Reflection;
using EFT;
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

        string formattedNum = reward.value.ToString("0.##");
        switch (reward.type)
        {
            //Reputation
            case (ERewardType)150:
            {
                typeText = "Rewards/Type/Stats".Localized();
                descriptionText = TaskRewardValuesTextGetter.GetLocalizedRewardDescription(reward);
                nameText = $"FactionName {reward.target}".Localized();
                string valueAdder = (reward.value > 0f) ? "+{0}" : "{0}";
                valueText = string.Format(valueAdder, formattedNum);
                descriptionText = string.Format(descriptionText, nameText, valueText);
                return false;
            }
            case (ERewardType)151:
            {
                typeText = reward.type.Localized();
                valueText = "";
                nameText = $"FactionName {reward.target}".Localized();
                descriptionText = "FactionDescription".Localized();
                return false;
            }
        }

        typeText = "";
        nameText = "";
        valueText = "";
        descriptionText = "";

        return true;
    }
}