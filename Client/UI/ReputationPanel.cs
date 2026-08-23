using System;
using System.Collections.Generic;
using EFT;
using EFT.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TerritoryClient.UI;

public class ReputationPanel : UIElement
{
    [SerializeField] public TextMeshProUGUI nameLabel;
    [SerializeField] public TextMeshProUGUI reputationLabel;
    [SerializeField] public TextMeshProUGUI hostilityLabel;

    [SerializeField] public Image baseFill;

    public readonly string HostileColor = "#E24E4E";
    public readonly string NeutralColor = "#CFC992";
    public readonly string AllyColor = "#7FDB69";
    
    public void Show(string faction, Dictionary<string, double> playerRep)
    {
        ShowGameObject();
        nameLabel.text = $"FactionName {faction}".Localized(EStringCase.Upper);
        
        //TODO: Color square to show faction color.
        
        if (!playerRep.TryGetValue(faction, out double repValue))
        {
            Plugin.PluginLogger.LogError($"Failed to find player rep for faction: {faction}");
            return;
        }

        reputationLabel.text = repValue.ToString("#0.00");

        string hostility = "HOSTILE";
        string color = HostileColor;

        if (repValue >= Plugin.StateManager.ServerData.AllyRep)
        {
            hostility = "FRIENDLY";
            color = AllyColor;
        }
        else if (repValue > Plugin.StateManager.ServerData.NeutralRep)
        {
            hostility = "NEUTRAL";
            color = NeutralColor;
        }

        hostilityLabel.text = $"<color={color}>{hostility.Localized()}</color>";
        baseFill.fillAmount = (float)Math.Clamp(repValue / Plugin.StateManager.ServerData.AllyRep, 0, 1);
    }
}