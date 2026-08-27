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
public class ApplyRewardOverride : AbstractPatch
{
    private static ReputationHelper ReputationHelper = null!;
    private static StateServer StateServer = null!;
    public static NotificationSendHelper NotificationSendHelper = null!;

    public ApplyRewardOverride(ReputationHelper reputationHelper, StateServer stateServer,
        NotificationSendHelper notificationSendHelper)
    {
        ReputationHelper = reputationHelper;
        StateServer = stateServer;
        NotificationSendHelper = notificationSendHelper;
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
                    NotificationSendHelper.SendMessageAsync(
                        fullProfile.ProfileInfo!.ProfileId!.Value,
                        new WsStateUpdateEvent
                        {
                            EventIdentifier = new MongoId(),
                            EventType = (NotificationEventType)100,
                            SaveState = StateServer.CurrentSave
                        }
                    );
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