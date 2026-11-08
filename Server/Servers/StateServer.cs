using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Servers.Ws;
using SPTarkov.Server.Core.Utils;
using TerritoryServer.Models;
using TerritoryServer.Models.Ws;
using TerritoryServer.Services;

namespace TerritoryServer.Servers;

[Injectable(InjectionType.Singleton)]
public class StateServer(JsonUtil jsonUtil,
    SptWebSocketConnectionHandler webSocketConnectionHandler,
    NotificationSendHelper notificationSendHelper,
    CacheService cacheService,
    TerritoryModConfig modConfig,
    TerritoryDataConfig dataConfig,
    ISptLogger<StateServer> logger)
{
    public static readonly string ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    private readonly string _locSavePath = Path.Join(ModPath, "territory.json");
    private readonly string _playerSavePath = Path.Join(ModPath, "players.json");

    public LocationSaveState CurrentLocationSave = null!;
    public PlayerSaveState CurrentPlayerSave = null!;
    public bool NewLocationSave = false;

    public async Task LoadSave()
    {
        LocationSaveState? tempSave = await jsonUtil.DeserializeFromFileAsync<LocationSaveState>(_locSavePath);

        if (tempSave == null)
        {
            logger.Info("[TT] No location save found. Creating new one.");
            tempSave = new LocationSaveState();
            NewLocationSave = true;
        }

        CurrentLocationSave = tempSave;
        
        PlayerSaveState? playerTempSave = await jsonUtil.DeserializeFromFileAsync<PlayerSaveState>(_playerSavePath);

        if (playerTempSave == null)
        {
            logger.Info("[TT] No player save found. Creating new one.");
            playerTempSave = new PlayerSaveState();
        }

        CurrentPlayerSave = playerTempSave;
        
        if (!NewLocationSave)
            ValidateState();
        
        SaveToDisk();
    }

    public void SaveToDisk()
    {
        CurrentLocationSave.StateId = new MongoId();
        File.WriteAllTextAsync(_locSavePath, jsonUtil.Serialize(CurrentLocationSave, true));
        File.WriteAllTextAsync(_playerSavePath, jsonUtil.Serialize(CurrentPlayerSave, true));
    }

    public void SendStateUpdate(MongoId? sessionId = null)
    {
        WsStateUpdateEvent message = new()
        {
            EventIdentifier = new MongoId(),
            EventType = (NotificationEventType)100,
            LocationSaveState = CurrentLocationSave,
            PlayerSaveState = CurrentPlayerSave
        };

        if (sessionId == null)
        {
            webSocketConnectionHandler.SendMessageToAll(message);
        }
        else
        {
            notificationSendHelper.SendMessageAsync(sessionId.Value, message);
        }
    }

    private void ValidateState()
    {
        foreach (string locationName in LocationService.MapList)
        {
            LocationState? locationState = CurrentLocationSave.Locations[locationName];
            if (locationState == null)
            {
                logger.Warning($"[TT] Location with id: {locationName} had no data! Generating.");
                CurrentLocationSave.Locations[locationName] = new LocationState
                {
                    Holder = "none",
                    BaseHolder = "none",
                    Contestants = []
                };
                continue;
            }

            foreach ((string factionId, double strength) in locationState.Contestants.ToList())
            {
                if (cacheService.ValidFactions.Contains(factionId))
                    continue;

                string replacement = dataConfig.Factions[locationState.Holder].ModSupport?.BackupFaction ?? "none";
                locationState.Contestants.Remove(factionId);
                
                if (replacement == "none")
                    continue;
                
                locationState.Contestants.TryAdd(replacement, strength);
            }

            if (!cacheService.ValidFactions.Contains(locationState.Holder))
            {
                if (locationState.Contestants.Count > 0)
                {
                    locationState.Holder = locationState.Contestants.First().Key;
                }
                else
                {
                    locationState.Holder = dataConfig.Factions[locationState.Holder].ModSupport?.BackupFaction 
                                           ?? "none";
                }
                
                locationState.BaseHolder = "none";
            }

            //if the holder doesn't have a contestant spot
            if (locationState.Holder != "none")
                locationState.Contestants.TryAdd(locationState.Holder, 0.01);
        }
        
        if (modConfig.Debug)
        {
            logger.Info("[TT] Finished sanitizing state file");
        }
    }
}