using System.Collections.Generic;
using System.Threading.Tasks;
using EFT;
using Newtonsoft.Json;
using SPT.Common.Http;
using TerritoryClient.Models;

namespace TerritoryClient.Services;

//keep track of each player kill and by the faction which killed them
public class KillCounter
{
    private Dictionary<string, int> _killCounter = [];
    private Dictionary<string, Dictionary<string, int>> _playerKillCounter = [];
    private string _location = "";
    private bool _raidActive;
    
    public void StartRaid(string location)
    {
        if (_raidActive)
        {
            Plugin.PluginLogger.LogError("Tried to start raid for kill counter with a raid already started!");
            return;
        }

        _location = location.ToLower();
        _raidActive = true;
        _killCounter.Clear();
        _playerKillCounter.Clear();
    }

    public void KilledEnemy(string botType, string? player = null)
    {
        if (!_raidActive)
            return;
        
        _killCounter.TryAdd(botType, 0);
        _killCounter[botType]++;

        Plugin.PluginLogger.LogInfo($"Bot: {botType} killed by player: {player}");
        
        if (player != null)
        {
            _playerKillCounter.TryAdd(player, []);
            
            _playerKillCounter[player].TryAdd(botType, 0);
            _playerKillCounter[player][botType]++;
        }
    }

    public async Task EndRaid()
    {
        if (!_raidActive)
        {
            return;
        }

        _raidActive = false;

        RaidStatRequest statRequest = new()
        {
            Kills = _killCounter,
            PlayerKills = _playerKillCounter,
            Location = _location
        };

        await RequestHandler.PutJsonAsync("/tt/match/end", JsonConvert.SerializeObject(statRequest));
    }
}