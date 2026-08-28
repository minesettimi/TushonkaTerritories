using System.Reflection;
using Microsoft.AspNetCore.Components;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Helpers.Commerce;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Enums;
using TerritoryServer.Helpers;
using TerritoryServer.Loaders;
using TerritoryServer.Models.Ws;
using TerritoryServer.Servers;

namespace TerritoryServer.Overrides;

[Injectable]
public class ApplyRewardPatch : AbstractPatch
{
    private static ProfileStateHelper _profileStateHelper = null!;
    private static StateServer StateServer = null!;

    public ApplyRewardPatch(ProfileStateHelper profileStateHelper, StateServer stateServer)
    {
        _profileStateHelper = profileStateHelper;
        StateServer = stateServer;
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
                    _profileStateHelper.AddRepToProfile(pmcData, reward.Target!, reward.Value!.Value);
                    tempRewards.RemoveAt(i--);
                    StateServer.SendStateUpdate(fullProfile.ProfileInfo!.ProfileId!);
                    break;
                
                case (RewardType)151:
                    PmcData? scavData = fullProfile.CharacterData?.ScavData;
                    
                    _profileStateHelper.SetFactionLock(pmcData, reward.Target!);
                    _profileStateHelper.SetFactionLock(scavData, reward.Target!);
                    tempRewards.RemoveAt(i--);
                    StateServer.SendStateUpdate(fullProfile.ProfileInfo!.ProfileId!);
                    break;
            }
        }

        rewards = tempRewards;
        StateServer.SaveToDisk();
    }
}