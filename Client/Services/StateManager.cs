using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SPT.Common.Http;
using TerritoryClient.Models;

namespace TerritoryClient.Services;

public class StateManager
{
    public ServerStateData State { get; private set; }

    public async Task<bool> RequestState()
    {
        try
        {
            string? data = await RequestHandler.GetJsonAsync("/tt/state/data");

            if (data != null)
            {
                State = JsonConvert.DeserializeObject<ServerStateData>(data)!;

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