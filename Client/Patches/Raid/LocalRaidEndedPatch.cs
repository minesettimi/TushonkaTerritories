using System.Reflection;
using System.Threading.Tasks;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.Raid;

public class LocalRaidEndedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EftClientBackendSession), nameof(EftClientBackendSession.LocalRaidEnded));
    }

    [PatchPostfix]
    public static async Task Postfix(Task __result)
    {
        await __result;
        
        await Plugin.KillCounter.EndRaid();
    }
}