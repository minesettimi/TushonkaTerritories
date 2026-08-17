using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.Patches.Debug;

public class CreateBotPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotCreatorClient), nameof(BotCreatorClient.CreateBot));
    }

    [PatchPrefix]
    public static void Prefix(Profile profile)
    {
        Plugin.PluginLogger.LogInfo($"Creating bot: {profile.Info.Settings.Role}");
    }
}