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
    [SerializeField] public GameObject pmcToggle = null!;
    [SerializeField] public GameObject scavToggle = null!;

    [SerializeField] public ReputationList reputationList = null!;

    private Profile _pmcProfile = null!;
    private Profile _scavProfile = null!;

    private UIAnimatedToggleSpawner _pmcToggle = null!;
    private UIAnimatedToggleSpawner _scavToggle = null!;
    private IImageLoader _session = null!;

    public void Awake()
    {
        _pmcToggle = pmcToggle.GetComponent<UIAnimatedToggleSpawner>();
        _scavToggle = scavToggle.GetComponent<UIAnimatedToggleSpawner>();

        _pmcToggle._object = CommonUIAwakePatch.TemplateToggle;
        _scavToggle._object = CommonUIAwakePatch.TemplateToggle;
        
        _pmcToggle.SpawnedObject.onValueChanged.AddListener(value => UpdateReputation(ReputationList.ERepProfile.PMC, value));
        _scavToggle.SpawnedObject.onValueChanged.AddListener(value => UpdateReputation(ReputationList.ERepProfile.Scav, value));
    }

    public void Show(Profile[] allProfiles, InventoryController inventoryController, IImageLoader session)
    {
        _session = session;
        
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
        reputationList.Show(profileType == ReputationList.ERepProfile.PMC ? _pmcProfile : _scavProfile, _session);
        UI.AddDisposable(reputationList);
    }

    public class ReputationTabController(
        ReputationScreen reputationTab,
        Profile[] allProfiles,
        InventoryController inventoryController,
        IImageLoader session)
        : UIElementTabController<ReputationScreen>(reputationTab)
    {
        public override void Show()
        {
            Content.Show(allProfiles, inventoryController, session);
        }
    }
}