using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using TerritoryServer.Helpers;
using TerritoryServer.Models;
using TerritoryServer.Servers;
using TerritoryServer.Services;

namespace TerritoryServer.Loaders;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 100000)]
public class PostLoad(LocationService locationService, ReputationService reputationService,
    LocaleService localeService,
    BattleService battleService,
    ClientEnumDefinitions clientEnumDefinitions,
    StateServer stateServer,
    BotTable botTable,
    TerritoryModConfig modConfig,
    ImageRouterHelper imageRouterHelper) : IOnLoad
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

        stateServer.NewSave = false;
        
        if (modConfig.RaidConfig.OverrideHostility)
            AdjustAllBots();
    }

    private void AdjustAllBots()
    {
        foreach ((string botName, BotType? botType) in botTable.Types)
        {
            foreach ((string difficultyName, DifficultyCategories difficulty) in botType.BotDifficulty)
            {
                //don't warn allies and don't leave shooting on warn up to chance
                difficulty.Patrol.MaxYDistToStartWarnRequestAlly = 0;
                difficulty.Boss.BossDistToShootAlly = 0;
                difficulty.Boss.BossDistToShootSqrtAlly = 0;
                difficulty.Boss.BossDistToWarningOutAlly = 0;
                difficulty.Boss.BossDistToWarningOutSqrtAlly = 0;
                difficulty.Boss.BossDistToWarningOutDeltaAlly = 0;
                difficulty.Boss.BossDistToWarningBearAlly = 0;
                difficulty.Boss.BossDistToWarningUsecAlly = 0;
                difficulty.Boss.BossDistToWarningAlly = 0;
                difficulty.Mind.ChanceShootWhenWarnPlayer100 = 0;
            }
        }
    }
}