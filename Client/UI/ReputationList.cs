using System.Collections.Generic;
using EFT;
using EFT.UI;
using UnityEngine;

namespace TerritoryClient.UI;

public class ReputationList : UIElement
{
    [SerializeField] public ReputationPanel repTemplate;
    [SerializeField] public Transform repContainer;
    
    public void Show(Profile profile)
    {
        if (!Plugin.StateManager.State.PlayerRep.TryGetValue(profile.Id, out Dictionary<string, double> playerRep))
        {
            Plugin.PluginLogger.LogError($"Failed to find player rep for id: {profile.Id}");
            return;
        }

        List<string> factions = [];

        foreach (string factionName in Plugin.StateManager.ServerData.FactionColors.Keys)
        {
            if (factionName != "none")
                factions.Add(factionName);
        }
        
        ShowGameObject();
        UI.AddViewList(factions, repTemplate,
            repContainer, (faction, panel) => panel.Show(faction, playerRep));
    }
    
    public enum ERepProfile
    {
        PMC,
        Scav
    }

}