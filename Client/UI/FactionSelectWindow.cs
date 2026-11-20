using System.Collections.Generic;
using EFT;
using EFT.UI;
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
            
            Close();
        });
    }

    public DialogWindowContext Show(List<string> selections)
    {
        DialogWindowContext context = base.Show();
        
        Caption.text = "FactionSelect".Localized();
        text.text = "FactionSelectDescription".Localized();

        List<string> selectionsList = [];
        foreach (string selection in selections)
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