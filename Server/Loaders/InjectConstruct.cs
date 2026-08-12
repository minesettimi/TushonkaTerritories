using System.Reflection;
using System.Text.Json;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;
using TerritoryServer.Models;

namespace TerritoryServer.Loaders;

public class InjectConstruct : IOnDIConstruct
{
    private static readonly string ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    
    public static async Task OnDIConstructAsync(IServiceCollection serviceCollection, CancellationToken cancellationToken)
    {
        DataConfig dataConfig = await LoadConfig<DataConfig>(Path.Join(ModPath, "Config", "data.json"), cancellationToken) ??
                                throw new Exception("[TT] Failed to load mod data.");

        string configPath = Path.Join(ModPath, "Config", "config.jsonc");
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