using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Ws;

namespace TerritoryServer.Models.Ws;

public record WsStateUpdateEvent : WsNotificationEvent
{
    [JsonPropertyName("locStateData")] public LocationSaveState LocationSaveState { get; set; } = null!;
    [JsonPropertyName("playerStateData")] public PlayerSaveState PlayerSaveState { get; set; } = null!;
}