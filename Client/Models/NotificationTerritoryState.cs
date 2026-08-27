using EFT;
using EFT.Communications;
using Newtonsoft.Json;

namespace TerritoryClient.Models;

public class NotificationTerritoryState : Notification
{
    public override string Description => "TerritoryUpdated".Localized();
    public override bool ShowNotification => false;

    [JsonProperty("stateData")] public ServerState ServerState = null!;
}