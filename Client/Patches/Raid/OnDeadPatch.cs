using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.Raid;

public class OnDeadPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.OnDead));
    }

    [PatchPostfix]
    public static void Postfix(Player __instance, IPlayer? __LastAggressor, bool __AggressorFound)
    {
        if (!__instance.IsAI)
            return;

        string role = __instance.Profile.Info.Settings.Role.ToString();
        
        if (__LastAggressor == null || __AggressorFound || __instance == (Player)__LastAggressor)
        {
            Plugin.KillCounter.KilledEnemy(role);
            return;
        }

        Player killer = __instance.GameWorld.GetAlivePlayerByProfileID(__LastAggressor.ProfileId);
        if (killer == null)
        {
            Plugin.KillCounter.KilledEnemy(role);
            return;
        }
        
        Plugin.KillCounter.KilledEnemy(role, killer.ProfileId);
    }
}