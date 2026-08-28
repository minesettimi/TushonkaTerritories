using System.Collections.Generic;
using EFT;
using EFT.UI;
using TerritoryClient.Models;
using UnityEngine;

namespace TerritoryClient.UI;

public class ReputationList : UIElement
{
    [SerializeField] public ReputationPanel repTemplate = null!;
    [SerializeField] public Transform repContainer = null!;
    
    public void Show(Profile profile, IImageLoader session)
    {
        if (!Plugin.StateManager.State.PlayerState.TryGetValue(profile.Id, out PlayerState playerState))
        {
            Plugin.PluginLogger.LogError($"Failed to find player rep for id: {profile.Id}");
            return;
        }

        //split it up for BSG's code
        List<string> factions = [];

        foreach (string factionName in Plugin.StateManager.ServerData.Factions.Keys)
        {
            if (factionName != "none")
            {
                factions.Add(factionName);
            }
        }
        
        ShowGameObject();
        UI.AddViewList(factions,
            repTemplate,
            repContainer,
            (faction,
                panel) => panel.Show(faction,
                Plugin.StateManager.ServerData.Factions[faction],
                playerState,
                session));
    }
    
    public enum ERepProfile
    {
        PMC,
        Scav
    }

}