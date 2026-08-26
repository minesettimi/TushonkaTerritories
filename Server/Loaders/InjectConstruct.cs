using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils.Json.Converters;
using TerritoryServer.Models;
using TerritoryServer.Servers;

namespace TerritoryServer.Loaders;

public class InjectConstruct : IOnDIConstruct
{
    public static readonly string ConfigPath = Path.Join(StateServer.ModPath, "Config");
    public static readonly string DataPath = Path.Join(StateServer.ModPath, "Data");

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        NewLine = "\n",
        Converters = { new StringToMongoIdConverter() }
    };

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
        ModConfig modConfig =
            await LoadConfig<ModConfig>(configPath, cancellationToken) ?? new ModConfig();

        //always save to update values
        await File.WriteAllTextAsync(configPath,
            JsonSerializer.Serialize(modConfig, _serializerOptions), cancellationToken);
        
        serviceCollection.AddSingleton(dataConfig);
        serviceCollection.AddSingleton(modConfig);
    }

    //from jsonutil but it can be used in a static context
    private static async Task<T?> LoadConfig<T>(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return default;
        }
        
        await using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);

        return await JsonSerializer.DeserializeAsync<T>(fs, _serializerOptions, cancellationToken);
    }
}