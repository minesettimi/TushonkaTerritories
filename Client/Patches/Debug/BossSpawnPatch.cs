using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.Debug;

public class BossSpawnPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotBossSpawn), nameof(BotBossSpawn.SpawnBossMain));
    }

    [PatchPrefix]
    public static void Prefix(GetProfileDataParams data)
    {
        Plugin.PluginLogger.LogInfo($"Creating bot with role: {data.Role}");
    }
}