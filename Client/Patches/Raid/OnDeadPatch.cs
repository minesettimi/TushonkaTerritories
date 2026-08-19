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

    [PatchPrefix]
    public static void Prefix(Player __instance, IPlayer? ___LastAggressor, bool ___AggressorFound)
    {
        if (!__instance.IsAI)
            return;

        string role = __instance.Profile.Info.Settings.Role.ToString();
        
        if (___LastAggressor == null || ___AggressorFound || __instance == (Player)___LastAggressor)
        {
            Plugin.KillCounter.KilledEnemy(role);
            return;
        }

        Player killer = __instance.GameWorld.GetAlivePlayerByProfileID(___LastAggressor.ProfileId);
        if (killer == null || killer.IsAI)
        {
            Plugin.KillCounter.KilledEnemy(role);
            return;
        }
        
        Plugin.KillCounter.KilledEnemy(role, killer.ProfileId);
    }
}