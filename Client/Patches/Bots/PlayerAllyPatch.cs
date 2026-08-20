using System.Collections.Generic;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.Models;

namespace TerritoryClient.Patches.Bots;

public class PlayerAllyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.IsAlly));
    }

    [PatchPrefix]
    public static bool Prefix(BotsGroup __instance, IPlayer player, ref bool __result)
    {
        ServerData data = Plugin.StateManager.ServerData;
        if (!data.AttitudeEffect || player.AIData.IsAI || player.IsAI)
            return true;

        string bossName = __instance._initialBot.Settings._role.ToString();
        string factionName = data.BotFaction.GetValueOrDefault(bossName, "none");
        
        if (!Plugin.StateManager.State.PlayerRep.TryGetValue(player.ProfileId,
                out Dictionary<string, double> playerRep))
        {
            return true;
        }

        double rep = playerRep.GetValueOrDefault(factionName, 0f);
        
        __result = rep >= data.AllyRep;
        return false;
    }
}