using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using TerritoryServer.Models;
using TerritoryServer.Servers;

namespace TerritoryServer.Controllers;

[Injectable(InjectionType.Singleton)]
public class ProfileController(StateServer stateServer,
    TerritoryDataConfig dataConfig,
    ProfileHelper profileHelper,
    ISptLogger<ProfileController> logger)
{
    public void ApplyRepPreset(string preset, MongoId sessionId)
    {
        if (!dataConfig.RepPresets.TryGetValue(preset, out RepPresetTable? repPreset))
        {
            logger.Error($"[TT] Could not find faction reputation preset: {preset}.", new Exception("Invalid preset"));
            return;
        }
        
        SptProfile fullProfile = profileHelper.GetFullProfile(sessionId);
        PmcData pmcProfile = fullProfile.CharacterData?.PmcData!;
        PmcData scavProfile = fullProfile.CharacterData?.ScavData!;
        MongoId pmcId = (MongoId)pmcProfile.Id!;
        
        ApplyRepPreset(pmcProfile.Info?.Side is Sides.Bear ? repPreset.Bear : repPreset.Usec, pmcId);
        ApplyRepPreset(repPreset.Scav, (MongoId)scavProfile.Id!);
        
        if (!stateServer.CurrentPlayerSave.PlayerState.TryGetValue(pmcId, out PlayerState? profileData))
        {
            logger.Error($"[TT] Could not get reputation for profile of id: {pmcId}!");
            return;
        }

        profileData.SelectedFaction = preset;
        stateServer.SaveToDisk();
        stateServer.SendStateUpdate(sessionId);
    }

    private void ApplyRepPreset(Dictionary<string, double> preset, MongoId profileId)
    {
        if (!stateServer.CurrentPlayerSave.PlayerState.TryGetValue(profileId, out PlayerState? profileData))
        {
            logger.Error($"[TT] Could not get reputation for profile of id: {profileId}!");
            return;
        }

        foreach ((string faction, double value) in preset)
        {
            profileData.Reputation[faction] = value;
        }
    }

    public string? GetSelectedFaction(MongoId sessionId)
    {
        PmcData? profile = profileHelper.GetPmcProfile(sessionId);

        if (profile == null) return null;
        
        MongoId profileId = (MongoId)profile.Id!;
            
        return stateServer.CurrentPlayerSave.PlayerState.TryGetValue(profileId,
            out PlayerState? value)
            ? value.SelectedFaction
            : null;
    }
}