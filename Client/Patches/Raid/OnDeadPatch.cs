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
            TerritoryPlugin.KillCounter.KilledEnemy(role);
            return;
        }

        if (___LastAggressor.IsAI || ___LastAggressor is not Player killer)
        {
            TerritoryPlugin.KillCounter.KilledEnemy(role);
            return;
        }

        if (!killer.IsYourPlayer && !killer.gameObject.name.StartsWith("Player_"))
        {
            TerritoryPlugin.KillCounter.KilledEnemy(role);
            return;
        }
        
        TerritoryPlugin.KillCounter.KilledEnemy(role, killer.ProfileId);
    }
}