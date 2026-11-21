using System.Collections.Generic;
using EFT;
using EFT.UI;
using Newtonsoft.Json;
using SPT.Common.Http;
using TerritoryClient.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace TerritoryClient.UI;

public class FactionSelectWindow : Window<DialogWindowContext>
{
    [SerializeField] public TextMeshProUGUI text = null!;
    [SerializeField] public DropDownBox dropDown = null!;
    [SerializeField] public DefaultUIButton confirmButton = null!;
    
    public int selectedIndex = -1;
    
    public override bool ShowOnTop => true;

    public override void Awake()
    {
        confirmButton.OnClick.AddListener(() =>
        {
            if (selectedIndex == -1)
                return;

            //change this temporarily just in case there's a network issue
            TerritoryPlugin.StateManager.State.SelectedFaction = "invalid";
            
            RequestHandler.PutJsonAsync("/tt/client/faction", 
                JsonConvert.SerializeObject(new FactionSelectRequest
                {
                    FactionName = TerritoryPlugin.StateManager.ServerData.PresetList[selectedIndex]
                }
            ));
            Close();
        });
    }

    public new DialogWindowContext Show()
    {
        DialogWindowContext context = base.Show();
        
        Caption.text = "FactionSelect".Localized();
        text.text = "FactionSelectDescription".Localized();
        
        List<string> selectionsList = [];
        foreach (string selection in TerritoryPlugin.StateManager.ServerData.PresetList)
        {
            selectionsList.Add($"PresetName {selection}".Localized());
        }

        confirmButton.Interactable = false;

        dropDown.Show(() => selectionsList);
        dropDown.UpdateValue(0, false);
        dropDown.Interactable = true;
        
        UI.SubscribeEvent(dropDown.OnValueChanged, (index) =>
        {
            selectedIndex = index;
            confirmButton.Interactable = true;
        });
        UI.AddDisposable(dropDown);
        
        WindowTransform.SetInCenter();
        this.WaitOneFrame(() => WindowTransform.SetInCenter());

        return context;
    }
}