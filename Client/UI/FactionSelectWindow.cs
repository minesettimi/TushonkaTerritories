using EFT.UI;
using TMPro;
using UnityEngine;

namespace TerritoryClient.UI;

public class FactionSelectWindow : Window<DialogWindowContext>
{
    [SerializeField] public TextMeshProUGUI Text;
    
    public override bool ShowOnTop => true;

    public DialogWindowContext Show(string title, string message)
    {
        DialogWindowContext context = base.Show();
        Text.text = message;
        Caption.text = title;
        
        WindowTransform.SetInCenter();
        this.WaitOneFrame(() => WindowTransform.SetInCenter());

        return context;
    }
}