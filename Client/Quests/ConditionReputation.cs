using EFT;
using EFT.Quests;

namespace TerritoryClient.Quests;

public class ConditionReputation : ConditionOneTarget
{
    public override string FormattedDescription =>
        string.Format("UI/Quests/Conditions/Reputation".Localized(),
            $"FactionName {target}".Localized());
}