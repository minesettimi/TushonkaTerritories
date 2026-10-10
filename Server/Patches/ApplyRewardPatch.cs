using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Helpers.Commerce;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using TerritoryServer.Helpers;
using TerritoryServer.Models;
using TerritoryServer.Servers;
using TerritoryServer.Services;

namespace TerritoryServer.Overrides;

[Injectable]
public class ApplyRewardPatch : AbstractPatch
{
    private static ProfileStateHelper _profileStateHelper = null!;
    private static StateServer _stateServer = null!;
    private static TerritoryDataConfig _dataConfig = null!;
    private static TerritoryModConfig _modConfig = null!;
    private static ISptLogger<ApplyRewardPatch> _logger = null!;

    public ApplyRewardPatch(ProfileStateHelper profileStateHelper, 
        StateServer stateServer,
        TerritoryDataConfig dataConfig,
        TerritoryModConfig modConfig,
        ISptLogger<ApplyRewardPatch> logger)
    {
        _profileStateHelper = profileStateHelper;
        _stateServer = stateServer;
        _dataConfig = dataConfig;
        _modConfig = modConfig;
        _logger = logger;
    }
    
    protected override MethodBase? GetTargetMethod()
    {
        return typeof(RewardHelper).GetMethod(nameof(RewardHelper.ApplyRewards));
    }

    [PatchPrefix]
    public static void Prefix(ref IEnumerable<Reward> rewards, SptProfile fullProfile, MongoId rewardSourceId)
    {
        PmcData? pmcData = fullProfile.CharacterData?.PmcData;
        if (pmcData == null)
            return;

        bool removeSaved = _stateServer.CurrentSave.QuestsCompleted.Contains(rewardSourceId);
        bool saved = false;
        List<Reward> tempRewards = [.. rewards];
        for (int i = 0; i < tempRewards.Count; i++)
        {
            Reward reward = tempRewards[i];
            
            if (reward.Type == null)
                continue;

            bool savableReward = _modConfig.OneTimeQuestRewards && _dataConfig.SavedQuestRewards.Contains((RewardType)reward.Type);
            bool custom = true;
            if (savableReward && !removeSaved)
            {
                saved = true;
                switch (reward.Type)
                {
                    case (RewardType)150:
                        _profileStateHelper.AddRepToProfile(pmcData, reward.Target!, reward.Value!.Value);
                        tempRewards.RemoveAt(i--);
                        _stateServer.SendStateUpdate(fullProfile.ProfileInfo!.ProfileId!);
                        break;
                
                    case (RewardType)151:
                        PmcData? scavData = fullProfile.CharacterData?.ScavData;
                    
                        _profileStateHelper.SetFactionLock(pmcData, reward.Target!);
                        _profileStateHelper.SetFactionLock(scavData, reward.Target!);
                        tempRewards.RemoveAt(i--);
                        _stateServer.SendStateUpdate(fullProfile.ProfileInfo!.ProfileId!);
                        break;
                
                    case (RewardType)152:
                        LocationState? locationState = _stateServer.CurrentSave.Locations[reward.Target!];
                    
                        if (locationState == null)
                        {
                            _logger.Error($"[TT] Tried to change lock on invalid map: {reward.Target}!");
                            break;
                        }

                        locationState.Locked = reward.Value < 1;
                        break;
                    default:
                        custom = false;
                        break;
                }
            }

            if (custom)
            {
                tempRewards.RemoveAt(i--);
            }
        }

        if (!removeSaved && saved)
        {
            _stateServer.CurrentSave.QuestsCompleted.Add(rewardSourceId);
        }

        rewards = tempRewards;
        _stateServer.SaveToDisk();
    }
}