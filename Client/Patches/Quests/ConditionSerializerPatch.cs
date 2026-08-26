using System.Reflection;
using EFT.Quests;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.Quests;

namespace TerritoryClient.Patches.Quests;

public class ConditionSerializerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Constructor(typeof(ConditionSerializer));
    }

    [PatchPostfix]
    public static void Postfix(ConditionSerializer __instance)
    {
        __instance._types.Add(typeof(ConditionReputation));
    }
}