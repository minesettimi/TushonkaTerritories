using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;
using TerritoryServer.Models;

namespace TerritoryServer.Servers;

[Injectable(InjectionType.Singleton)]
public class StateServer(JsonUtil jsonUtil,
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
        File.WriteAllTextAsync(_savePath, jsonUtil.Serialize(CurrentSave, true));
    }
}