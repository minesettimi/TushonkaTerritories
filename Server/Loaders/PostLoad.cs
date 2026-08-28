using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Utils.Json;
using TerritoryServer.Helpers;
using TerritoryServer.Services;

namespace TerritoryServer.Loaders;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 100000)]
public class PostLoad(LocationService locationService, ReputationService reputationService,
    LocaleService localeService,
    BattleService battleService,
    ClientEnumDefinitions clientEnumDefinitions,
    ImageRouterHelper imageRouterHelper,
    TemplateTable templateTable) : IOnLoad
{
    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        locationService.Initialize();
        reputationService.CheckRep();
        await localeService.Load();
        
        battleService.Setup();
        
        clientEnumDefinitions.AddRange(
            "com.minesettimi.territories",
            [
                new EnumEntryDefinition
                {
                    EnumType = "EFT.UI.EInventoryTab",
                    ConstantName = "Reputation",
                    ConstantValue = 8
                },
                new EnumEntryDefinition
                {
                    EnumType = "EFT.Quests.ERewardType",
                    ConstantName = "Reputation",
                    ConstantValue = 150
                },
                new EnumEntryDefinition
                {
                    EnumType = "EFT.Quests.ERewardType",
                    ConstantName = "FactionUnlock",
                    ConstantValue = 151
                },
                new EnumEntryDefinition
                {
                    EnumType = "EFT.Communications.ENotificationType",
                    ConstantName = "TerritoryUpdate",
                    ConstantValue = 100
                }
            ]
        );
        
        imageRouterHelper.LoadFactionImages();
        
        //debug
        templateTable.Quests["5a27b9de86f77464e5044585"].Rewards?["Success"].Add(new Reward
        {
            AvailableInGameEditions = [],
            GameMode = ["regular", "pve"],
            Id = new MongoId(),
            IsHidden = false,
            Type = (RewardType)150,
            Unknown = false,
            Target = "cultist",
            Value = 0.3
        });
        
        templateTable.Quests["5a27b9de86f77464e5044585"].Rewards?["Success"].Add(new Reward
        {
            AvailableInGameEditions = [],
            GameMode = ["regular", "pve"],
            Id = new MongoId(),
            IsHidden = false,
            Type = (RewardType)151,
            Unknown = false,
            Target = "cultist"
        });
        
        templateTable.Quests["5a27b9de86f77464e5044585"].Conditions.AvailableForFinish?.Add(new QuestCondition
        {
            ConditionType = "Reputation",
            DynamicLocale = false,
            GlobalQuestCounterId = "",
            Id = "69631a0bc6c5f245d93d25c2",
            Index = 2,
            OneSessionOnly = false,
            IsEncoded = false,
            ParentId = "",
            Type = "TraderStanding",
            Target = new ListOrT<string>(null, "cultist"),
            Value = 7.5,
            VisibilityConditions = []
        });
    }
}