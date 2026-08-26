using System.Reflection;
using EFT.Quests;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.Quests;

namespace TerritoryClient.Patches.Quests;

public class ConditionCounterPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ConditionalHelper), nameof(ConditionalHelper.ConditionIsCounter));
    }

    [PatchPrefix]
    public static bool Prefix(Condition condition, ref bool __result)
    {
        if (condition is ConditionReputation)
        {
            __result = true;
            return false;
        }

        return true;
    }
}