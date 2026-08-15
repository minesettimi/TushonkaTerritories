using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Utils.Cloners;
using TerritoryServer.Models;
using TerritoryServer.Servers;

namespace TerritoryServer.Services;

[Injectable(InjectionType.Singleton)]
public class LocationService(DataConfig dataConfig,
    LocationTable locationTable,
    ModConfig modConfig,
    StateServer stateServer,
    BotConfig botConfig,
    LocationConfig locationConfig,
    BotTable botTable,
    ICloner cloner)
{
    public static readonly List<string> MapList = 
    [
        "bigmap",
        "factory4_day",
        "interchange",
        "laboratory",
        "lighthouse",
        "reservbase",
        "sandbox",
        "shoreline",
        "tarkovstreets",
        "woods",
        "labyrinth",
        "suburbs",
        "terminal"
    ];

    private static readonly List<string> Difficulties =
    [
        "impossible",
        "hard",
        "normal",
        "easy"
    ];

    enum BotRelationship
    {
        Friends,
        Neutral,
        Enemies
    }

    private Dictionary<string, BossLocationSpawn> _mobileBossData = [];
    
    private Dictionary<string, List<BossLocationSpawn>> _bossBackup = [];
    private Dictionary<string, List<Wave>> _waveBackup = [];
    private Dictionary<string, IEnumerable<AdditionalHostilitySettings>> _hostilityBackup = [];
    private IEnumerable<AdditionalHostilitySettings> _hostilityCache = [];

    public void Initialize()
    {
        BackupLocationData();
        AdjustLocationSettings();
        BuildHostilityCache();
        UpdateLocations();
    }
    
    private void BackupLocationData()
    {
        List<string> bossesToRemove = [];

        foreach (Faction faction in dataConfig.Factions.Values)
        {
            bossesToRemove.AddRange(faction.BossNames);
        }

        foreach ((string locationName, Location locationInfo) in locationTable.GetDictionary())
        {
            _bossBackup.Add(locationName, []);
            foreach (BossLocationSpawn bossSpawn in locationInfo.Base.BossLocationSpawn)
            {
                BossLocationSpawn clonedSpawn = cloner.Clone(bossSpawn)!;
                if (bossesToRemove.Contains(bossSpawn.BossName!))
                {
                    clonedSpawn.TriggerId = "";
                    clonedSpawn.TriggerName = "";
                    clonedSpawn.BossZone = "";
                    clonedSpawn.ShowOnTarkovMap = false;
                    clonedSpawn.ShowOnTarkovMapPvE = false;
                    
                    _mobileBossData.Add(clonedSpawn.BossName!, clonedSpawn);
                    continue;
                }
                
                _bossBackup[locationName].Add(clonedSpawn);
            }
            
            _waveBackup.Add(locationName, cloner.Clone(locationInfo.Base.Waves)!);
            _hostilityBackup.Add(locationName,
                cloner.Clone(locationInfo.Base.BotLocationModifier.AdditionalHostilitySettings)!);
        }
    }

    //credit to acidphantasm for originally finding the data that needed to be changed
    private void AdjustLocationSettings()
    {
        locationConfig.AddCustomBotWavesToMaps = false;
        locationConfig.EnableBotTypeLimits = false;
        locationConfig.AddOpenZonesToAllMaps = false;
        locationConfig.RogueLighthouseSpawnTimeSettings.Enabled = false;
        
        foreach (string locationName in MapList)
        {
            LocationBase location = locationTable.GetLocation(locationName)!.Base;

            for(int i = 0; i < location.BossLocationSpawn.Count; i++)
            {
                BossLocationSpawn bossSpawn = location.BossLocationSpawn[i];

                bool isPmc = bossSpawn.BossName == "pmcBear" || bossSpawn.BossName == "pmcUSEC";
                
                if (modConfig.RaidConfig.OverridePmcs && isPmc || modConfig.RaidConfig.OverrideBosses && !isPmc)
                {
                    location.BossLocationSpawn.RemoveAt(i);
                }
            }
            
            location.Waves = [];
            location.NewSpawn = false;
            location.OfflineNewSpawn = false;
            location.OldSpawn = true;
            location.OfflineOldSpawn = true;

            if (!botConfig.PlayerScavBrainType.ContainsKey(locationName))
            {
                botConfig.PlayerScavBrainType.Add(locationName, cloner.Clone(botConfig.PlayerScavBrainType["tarkovstreets"])!);
            }

            if (!botConfig.AssaultBrainType.ContainsKey(locationName))
            {
                botConfig.AssaultBrainType.Add(locationName, cloner.Clone(botConfig.AssaultBrainType["tarkovstreets"])!);
            }
        }
    }
    
    //null updates all valid locations
    public void UpdateLocations(List<string>? maps = null)
    {
        foreach (string locationName in maps ?? MapList)
        {
            LocationBase location = locationTable.GetLocation(locationName)!.Base;
            
            LocationState locationState = stateServer.CurrentSave.Locations[locationName]!;

            //re-uses previous configuration if there's no faction, change if not desired
            if (locationState.Holder == "none")
                continue;
            
            Faction faction = dataConfig.Factions[locationState.Holder];
            
            if (modConfig.RaidConfig.FactionBosses)
            {
                location.BossLocationSpawn = cloner.Clone(_bossBackup[locationName])!;
                    
                foreach ((string bossName, BossLocationSpawn bossSpawn) in _mobileBossData)
                {
                    if (!faction.BossNames.Contains(bossName))
                        continue;
                    
                    location.BossLocationSpawn.Add(cloner.Clone(bossSpawn)!);
                }
            }
            
            if (modConfig.RaidConfig.AttitudeEffect)
            {
                location.BotLocationModifier.AdditionalHostilitySettings = cloner.Clone(_hostilityCache)!;
            }

            if (modConfig.RaidConfig.OverrideWaves)
                location.Waves.Clear();
            else
                location.Waves = cloner.Clone(_waveBackup[locationName])!;
        }
        
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
    }
    
    private void BuildHostilityCache()
    {
        List<AdditionalHostilitySettings> finalSettings = [];
        
        foreach ((string botName, string botFaction) in dataConfig.BotFaction)
        {
            //player behavior is handled by a patch, don't mess with it
            AdditionalHostilitySettings newSettings = new()
            {
                AlwaysEnemies = [],
                AlwaysFriends = [],
                BotRole = botName,
                ChancedEnemies = [],
                Neutral = []
            };

            List<string> warnList = [];

            foreach ((string otherBot, string otherFaction) in dataConfig.BotFaction)
            {
                BotRelationship relationship = GetBotRelationship(botName, otherBot);

                switch (relationship)
                {
                    case BotRelationship.Enemies:
                        newSettings.AlwaysEnemies.Add(otherBot);
                        continue;
                    case BotRelationship.Friends:
                        newSettings.AlwaysFriends.Add(otherBot);
                        continue;
                    case BotRelationship.Neutral:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                switch (modConfig.RaidConfig.NeutralityMode)
                {
                    case NeutralMode.ChancedEnemies:
                        newSettings.ChancedEnemies.Add(new ChancedEnemy
                        {
                            EnemyChance = modConfig.RaidConfig.EnemyChance,
                            Role = otherBot
                        });
                        break;
                    case NeutralMode.Warn:
                        warnList.Add(otherBot);
                        break;
                    case NeutralMode.Neutral:
                    default:
                        newSettings.Neutral.Add(otherBot);
                        break;
                }
                
            }

            newSettings.Warn = warnList;
            finalSettings.Add(newSettings);
        }

        _hostilityCache = finalSettings;
    }

    private BotRelationship GetBotRelationship(string botName, string otherBot)
    {
        string factionName = dataConfig.BotFaction[botName];
        
        Faction factionData = dataConfig.Factions[factionName];
        
        string otherFaction = dataConfig.BotFaction[otherBot];
        if (factionName == otherFaction)
            return BotRelationship.Friends;

        int attitude = factionData.Attitudes[otherFaction];

        return attitude switch
        {
            1 => BotRelationship.Friends,
            0 => BotRelationship.Neutral,
            _ => BotRelationship.Enemies
        };
    }
}