using System.Reflection;
using System.Threading.Tasks;
using EFT;
using EFT.Quests;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.Models;
using TerritoryClient.UI;

namespace TerritoryClient.Patches.Quests;

public class TraderRewardViewPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(TraderRewardView), nameof(TraderRewardView.Show));
    }

    [PatchPrefix]
    public static bool Show(TraderRewardView __instance, ref SimpleTooltip ____tooltip, QuestReward reward)
    {
        if (reward.type != (ERewardType)151)
            return true;

        ____tooltip = ItemUiContext.Instance.Tooltip;
        __instance.ShowGameObject();

        TaskRewardValuesTextGetter.GetRewardValuesText(reward, out string _, out string nameText, out string _, out string _);
        __instance._traderName.text = nameText;

        FactionData factionData = Plugin.StateManager.ServerData.Factions[reward.target];

        if (factionData.Sprite == null) return false;
        
        __instance._avatar.sprite = factionData.Sprite;
        _ = TryLoadIcon(__instance, factionData, QuestViewShowPatch.BackendSession, reward.target);
        
        return false;

    }
    
    private static async Task TryLoadIcon(TraderRewardView instance, FactionData factionData, IImageLoader session, string faction)
    {
        await factionData.LoadSprite(session, faction);
        if (factionData.Sprite != null)
        {
            instance._avatar.sprite = factionData.Sprite;
        }
    }
}