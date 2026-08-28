using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EFT;
using EFT.UI;
using TerritoryClient.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryClient.UI;

public class ReputationPanel : UIElement
{
    [SerializeField] public TextMeshProUGUI nameLabel = null!;
    [SerializeField] public TextMeshProUGUI reputationLabel = null!;
    [SerializeField] public TextMeshProUGUI hostilityLabel = null!;

    [SerializeField] public Image baseFill = null!;
    [SerializeField] public Image factionColor = null!;
    [SerializeField] public Image factionImage = null!;
    [SerializeField] public Image lockImage = null!;
    [SerializeField] public Image unlockImage = null!;

    public readonly string HostileColor = "#E24E4E";
    public readonly string NeutralColor = "#CFC992";
    public readonly string AllyColor = "#7FDB69";
    
    public void Show(string faction, FactionData factionData, 
        PlayerState playerState, IImageLoader session)
    {
        ShowGameObject();
        nameLabel.text = $"FactionName {faction}".Localized(EStringCase.Upper);
        
        ServerData serverData = Plugin.StateManager.ServerData;
        
        factionColor.color = factionData.Color;

        switch (factionData.Locked)
        {
            case true when !playerState.Unlocked[faction]:
                lockImage.enabled = true;
                unlockImage.enabled = false;
                break;
            case true when playerState.Unlocked[faction]:
                lockImage.enabled = false;
                unlockImage.enabled = true;
                break;
            default:
                lockImage.enabled = false;
                unlockImage.enabled = false;
                break;
        }

        if (!playerState.Reputation.TryGetValue(faction, out double repValue))
        {
            Plugin.PluginLogger.LogError($"Failed to find player rep for faction: {faction}");
            return;
        }

        reputationLabel.text = repValue.ToString("#0.00");

        string hostility = "HOSTILE";
        string color = HostileColor;

        if (repValue >= serverData.AllyRep)
        {
            hostility = "FRIENDLY";
            color = AllyColor;
        }
        else if (repValue > serverData.NeutralRep)
        {
            hostility = "NEUTRAL";
            color = NeutralColor;
        }

        hostilityLabel.text = $"<color={color}>{hostility.Localized()}</color>";
        baseFill.fillAmount = (float)Math.Clamp(repValue / serverData.AllyRep, 0, 1);

        if (factionData.Image == null)
            return;

        if (factionData.Sprite != null)
        {
            factionImage.sprite = factionData.Sprite;
            return;
        }

        _ = TryLoadIcon(factionData, session);
    }

    private async Task TryLoadIcon(FactionData factionData, IImageLoader session)
    {
        await factionData.LoadSprite(session);
        if (factionData.Sprite != null)
        {
            factionImage.sprite = factionData.Sprite;
        }
    }
}