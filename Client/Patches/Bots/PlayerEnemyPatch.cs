using System.Collections.Generic;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.Models;

namespace TerritoryClient.Patches.Bots;


public class PlayerEnemyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.IsPlayerEnemy));
    }

    [PatchPrefix]
    public static bool Prefix(BotsGroup __instance, IPlayer player, ref bool __result)
    {
        ServerData data = Plugin.StateManager.ServerData;
        if (!data.AttitudeEffect || player.AIData.IsAI || player.IsAI)
            return true;

        string bossName = __instance.InitialBotType.ToString();
        string factionName = data.BotFaction.GetValueOrDefault(bossName, "none");
        
        Dictionary<string, double> playerRep = Plugin.StateManager.State.PlayerRep[player.ProfileId];

        double rep = playerRep[factionName];

        if (rep < data.NeutralRep)
            return true;

        __result = false;
        return false;
    }
}