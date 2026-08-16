using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SPT.Common.Http;
using TerritoryClient.Models;

namespace TerritoryClient.Services;

//keep track of each player kill and by the faction which killed them
public class KillCounter
{
    private Dictionary<string, int> _killCounter = [];
    private string _profileId = "";
    private string _location = "";
    private bool _raidActive;
    
    public void StartRaid(string profileId, string location)
    {
        if (_raidActive)
        {
            Plugin.PluginLogger.LogError("Tried to start raid for kill counter with a raid already started!");
            return;
        }

        _location = location;
        _raidActive = true;
        _killCounter = [];
        _profileId = profileId;
    }

    public void KilledEnemy(string botType)
    {
        _killCounter.TryAdd(botType, 0);

        _killCounter[botType]++;
    }

    public async Task EndRaid()
    {
        if (!_raidActive)
        {
            Plugin.PluginLogger.LogError("Tried to end raid for kill counter without the raid starting!");
            return;
        }

        _raidActive = false;

        RaidStatRequest statRequest = new()
        {
            Kills = _killCounter,
            ProfileId = _profileId,
            Location = _location
        };

        await RequestHandler.PutJsonAsync("/tt/match/end", JsonConvert.SerializeObject(statRequest));
    }
}