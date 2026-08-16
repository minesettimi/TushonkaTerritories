using System.Text.Json;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;
using TerritoryServer.Models;
using TerritoryServer.Servers;

namespace TerritoryServer.Loaders;

public class InjectConstruct : IOnDIConstruct
{
    private static readonly string ConfigPath = Path.Join(StateServer.ModPath, "Config");
    private static readonly string DataPath = Path.Join(StateServer.ModPath, "Data");

    public static async Task OnDIConstructAsync(IServiceCollection serviceCollection, CancellationToken cancellationToken)
    {
        DataConfig dataConfig;

        if (File.Exists(Path.Join(DataPath, "data_override.json")))
        {
            dataConfig = await LoadConfig<DataConfig>(Path.Join(DataPath, "data_override.json"), cancellationToken) ??
                throw new Exception("[TT] Failed to load override mod data.");
        }
        else
        {
            dataConfig = await LoadConfig<DataConfig>(Path.Join(DataPath, "data.json"), cancellationToken) ??
                         throw new Exception("[TT] Failed to load mod data.");
        }
        
        if (!Directory.Exists(ConfigPath))
        {
            Directory.CreateDirectory(ConfigPath);
        }
        
        string configPath = Path.Join(ConfigPath, "config.jsonc");
        ModConfig? modConfig =
            await LoadConfig<ModConfig>(configPath, cancellationToken);

        if (modConfig == null)
        {
            modConfig = new ModConfig();
            await File.WriteAllTextAsync(configPath,
                JsonSerializer.Serialize(modConfig, JsonUtil.JsonSerializerOptionsIndented), cancellationToken);
        }
        
        serviceCollection.AddSingleton(dataConfig);
        serviceCollection.AddSingleton(modConfig);
    }

    //from jsonutil but it can be used in a static context
    public static async Task<T?> LoadConfig<T>(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return default;
        }
        
        await using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);

        return await JsonSerializer.DeserializeAsync<T>(fs, JsonUtil.JsonSerializerOptionsIndented, cancellationToken);
    }
}