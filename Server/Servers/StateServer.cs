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
    ModConfig modConfig,
    ISptLogger<StateServer> logger)
{
    public static readonly string ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    private readonly string _savePath = Path.Join(ModPath, "save.json");

    public SaveState CurrentSave = null!;
    public bool NewSave = false;

    public async Task LoadSave()
    {
        SaveState? tempSave = await jsonUtil.DeserializeFromFileAsync<SaveState>(_savePath);

        if (tempSave == null)
        {
            logger.Info("[TT] No save found. Creating new one.");
            tempSave = new SaveState();
            NewSave = true;
        }

        CurrentSave = tempSave;
        
        if (!NewSave)
            ValidateState();
        
        SaveToDisk();
    }

    public void SaveToDisk()
    {
        CurrentSave.StateId = new MongoId();
        File.WriteAllTextAsync(_savePath, jsonUtil.Serialize(CurrentSave, true));
    }

    public void SendStateUpdate(MongoId? sessionId = null)
    {
        WsStateUpdateEvent message = new()
        {
            EventIdentifier = new MongoId(),
            EventType = (NotificationEventType)100,
            SaveState = CurrentSave
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
            LocationState? locationState = CurrentSave.Locations[locationName];
            if (locationState == null)
            {
                logger.Warning($"[TT] Location with id: {locationName} had no data! Generating.");
                CurrentSave.Locations[locationName] = new LocationState
                {
                    Holder = "none",
                    Base = false,
                    Contestants = []
                };
                continue;
            }

            foreach ((string factionId, double strength) in locationState.Contestants)
            {
                if (cacheService.ValidFactions.Contains(factionId))
                    continue;

                locationState.Contestants.Remove(factionId);
            }

            if (!cacheService.ValidFactions.Contains(locationState.Holder))
            {
                if (locationState.Contestants.Count > 0)
                {
                    locationState.Holder = locationState.Contestants.First().Key;
                }
                else
                {
                    locationState.Holder = "none";
                }
                
                locationState.Base = false;
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