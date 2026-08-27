using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Ws;

namespace TerritoryServer.Models.Ws;

public record WsStateUpdateEvent : WsNotificationEvent
{
    [JsonPropertyName("stateData")] public SaveState SaveState { get; set; } = null!;
}