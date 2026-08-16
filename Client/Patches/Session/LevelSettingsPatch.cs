using System.Reflection;
using System.Threading.Tasks;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.Session;

public class LevelSettingsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EftClientBackendSession), nameof(EftClientBackendSession.GetLevelSettings));
    }

    [PatchPrefix]
    public static async Task Prefix()
    {
        await Plugin.StateManager.RequestState();
    }
}