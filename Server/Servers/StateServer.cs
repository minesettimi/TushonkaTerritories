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

namespace TerritoryServer.Servers;

[Injectable(InjectionType.Singleton)]
public class StateServer(JsonUtil jsonUtil,
    SptWebSocketConnectionHandler webSocketConnectionHandler,
    NotificationSendHelper notificationSendHelper,
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
}