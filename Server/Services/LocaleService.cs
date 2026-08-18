using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json;
using TerritoryServer.Loaders;

namespace TerritoryServer.Services;

[Injectable(InjectionType.Singleton)]
public class LocaleService(FileUtil fileUtil, 
    JsonUtil jsonUtil, 
    LocaleTable localeTable,
    ISptLogger<LocaleService> logger)
{
    public async Task Load()
    {
        string localeDir = Path.Join(InjectConstruct.DataPath, "Locales");

        List<string> localeFiles = fileUtil.GetFiles(localeDir, false, "*.json");

        Dictionary<string, Dictionary<string, string>?> locales = new();
        
        foreach (string file in localeFiles)
        {
            string langCode = Path.GetFileNameWithoutExtension(file);

            Dictionary<string, string>? localeData = await jsonUtil.DeserializeFromFileAsync<Dictionary<string, string>>(file);

            if (localeData == null)
            {
                logger.Warning($"[TT] Failed to load locale file {file}");
                localeData = [];
            }
            
            locales[langCode] = localeData;
        }

        foreach ((string locale, LazyLoad<GlobalLocaleDictionary> lazyLoadedVal) in localeTable.Global)
        {
            lazyLoadedVal.AddTransformer(localeData =>
            {
                if (localeData == null) 
                    return localeData;

                locales.TryGetValue(locale, out Dictionary<string, string>? customLocales);

                if (customLocales == null)
                    return localeData;

                foreach ((string key, string value) in customLocales)
                    localeData[key] = value;

                return localeData;
            });
        }
    }
}