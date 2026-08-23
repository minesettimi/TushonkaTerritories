using System;
using System.Linq;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using TerritoryClient.Patches.UI;
using UnityEngine;
using UnityEngine.Events;

namespace TerritoryClient.UI;

public class ReputationScreen : UIElement
{
    [SerializeField] public GameObject pmcToggle;
    [SerializeField] public GameObject scavToggle;

    [SerializeField] public ReputationList reputationList;

    private Profile _pmcProfile;
    private Profile _scavProfile;

    private UIAnimatedToggleSpawner _pmcToggle;
    private UIAnimatedToggleSpawner _scavToggle;

    public void Awake()
    {
        _pmcToggle = pmcToggle.GetComponent<UIAnimatedToggleSpawner>();
        _scavToggle = scavToggle.GetComponent<UIAnimatedToggleSpawner>();

        _pmcToggle._object = CommonUIAwakePatch.TemplateToggle;
        _scavToggle._object = CommonUIAwakePatch.TemplateToggle;
        
        _pmcToggle.SpawnedObject.onValueChanged.AddListener(value => UpdateReputation(ReputationList.ERepProfile.PMC, value));
        _scavToggle.SpawnedObject.onValueChanged.AddListener(value => UpdateReputation(ReputationList.ERepProfile.Scav, value));
    }

    public void Show(Profile[] allProfiles, InventoryController inventoryController)
    {
        inventoryController.StopProcesses();
        ItemUiContext.Instance.CloseAllWindows();

        //idk how its ordered or if that order gets messed up, id hate to use LINQ on the client but fuck it
        _pmcProfile = allProfiles.First(profile => profile.Info.Side.CheckSide(EPlayerSideMask.Pmc));
        _scavProfile = allProfiles.First(profile => profile.Info.Side.CheckSide(EPlayerSideMask.Savage));
        
        UI.Dispose();
        ShowGameObject();
        _pmcToggle.IsToggled = true;
        
        UpdateReputation(ReputationList.ERepProfile.PMC, true);
    }

    public void UpdateReputation(ReputationList.ERepProfile profileType, bool value)
    {
        if (!value)
            return;
        
        reputationList.Dispose();
        reputationList.Show(profileType == ReputationList.ERepProfile.PMC ? _pmcProfile : _scavProfile);
        UI.AddDisposable(reputationList);
    }

    public class ReputationTabController(
        ReputationScreen reputationTab,
        Profile[] allProfiles,
        InventoryController inventoryController)
        : UIElementTabController<ReputationScreen>(reputationTab)
    {
        public override void Show()
        {
            Content.Show(allProfiles, inventoryController);
        }
    }
}