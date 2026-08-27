using System.Reflection;
using EFT.Communications;
using HarmonyLib;
using SPT.Reflection.Patching;
using TerritoryClient.Models;

namespace TerritoryClient.Patches.Communications;

public class NotificationParsePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Notifications), nameof(Notifications.ParseNotificationByType));
    }

    [PatchPrefix]
    public static bool Prefix(ENotificationType type, UnparsedData data, ref Notification __result)
    {
        switch (type)
        {
            case (ENotificationType)100:

                __result = data.ParseJsonTo<NotificationTerritoryState>();
                return false;
        }

        return true;
    }
}