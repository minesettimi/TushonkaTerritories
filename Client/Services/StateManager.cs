using System;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.Communications;
using Newtonsoft.Json;
using SPT.Common.Http;
using TerritoryClient.Models;

namespace TerritoryClient.Services;

public class StateManager
{
    public ServerData ServerData { get; private set; }
    public ServerState State { get; private set; }

    public Action StateUpdated;

    public void Init()
    {
        _ = RequestState();
        if (Singleton<NotificationManager>.Instantiated)
        {

            Singleton<NotificationManager>.Instance.OnNotificationReceived += notification =>
            {
                if (notification is NotificationTerritoryState territoryNotification)
                {
                    State = territoryNotification.ServerState;
                }
            };
        }
    }


    public async Task<bool> RequestData()
    {
        try
        {
            string? data = await RequestHandler.GetJsonAsync("/tt/state/data");

            if (data != null)
            {
                ServerData = JsonConvert.DeserializeObject<ServerData>(data)!;

                return true;
            }
        }
        catch (Exception e)
        {
            Plugin.PluginLogger.LogError($"Failed to get data from server with error: {e.Message}");
            throw;
        }

        return false;
    }
    
    public async Task<bool> RequestState()
    {
        try
        {
            string? data = await RequestHandler.GetJsonAsync("/tt/state/server");

            if (data != null)
            {
                State = JsonConvert.DeserializeObject<ServerState>(data)!;
                
                StateUpdated?.Invoke();
                return true;
            }
        }
        catch (Exception e)
        {
            Plugin.PluginLogger.LogError($"Failed to get state from server with error: {e.Message}");
            throw;
        }

        return false;
    }
}