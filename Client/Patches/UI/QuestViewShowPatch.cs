using System.Reflection;
using EFT;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TerritoryClient.UI;

public class QuestViewShowPatch : ModulePatch
{
    //probably a bad idea to do it like this... too bad!
    public static IEftSession BackendSession;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(QuestView), nameof(QuestView.Show));
    }

    [PatchPrefix]
    public static void Prefix(IEftSession backendSession)
    {
        BackendSession = backendSession;
    }
}