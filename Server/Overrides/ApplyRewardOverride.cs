using System.Reflection;
using Microsoft.AspNetCore.Components;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Helpers.Commerce;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using TerritoryServer.Helpers;
using TerritoryServer.Loaders;
using TerritoryServer.Servers;

namespace TerritoryServer.Overrides;

[Injectable]
public class ApplyRewardOverride : AbstractPatch
{
    public static ReputationHelper ReputationHelper = null!;
    public static StateServer StateServer = null!;

    public ApplyRewardOverride(ReputationHelper reputationHelper)
    {
        ReputationHelper = reputationHelper;
    }
    
    protected override MethodBase? GetTargetMethod()
    {
        return typeof(RewardHelper).GetMethod(nameof(RewardHelper.ApplyRewards));
    }

    [PatchPrefix]
    public static void Prefix(ref IEnumerable<Reward> rewards, SptProfile fullProfile)
    {
        PmcData? pmcData = fullProfile.CharacterData?.PmcData;
        if (pmcData == null)
            return;

        List<Reward> tempRewards = [.. rewards];
        for (int i = 0; i < tempRewards.Count; i++)
        {
            Reward reward = tempRewards[i];

            switch (reward.Type)
            {
                case (RewardType)150:
                    ReputationHelper.AddRepToProfile(pmcData, reward.Target!, reward.Value!.Value);
                    tempRewards.RemoveAt(i--);
                    break;
                
                case (RewardType)151:
                    //TODO: Implement unlockable factions
                    tempRewards.RemoveAt(i--);
                    break;
            }
        }

        rewards = tempRewards;
        StateServer.SaveToDisk();
    }
}