using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponsMenuController : FullScreenMenuController
{
    public GameOptionsSO Options;

    [SerializeField]
    private Button _closeButton;
    public Button CloseButton => _closeButton;

    public GameObject ShopMenu;
    public WeaponShopCell ShopCellPrefab;
    public TMP_Text TotalCostText;
    public List<WeaponShopCell> WeaponCells;

    public override FullscreenMenuType Type { get => FullscreenMenuType.Weapons; }

    private void Start()
    {
        foreach (var weaponCell in WeaponCells)
        {
            weaponCell.OnCostChanged += OnCostChangedReceived;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(ShopMenu.GetComponent<RectTransform>());

    }
    public override void EnableControls(bool enabled)
    {
        CloseButton.enabled = enabled;
    }

    private void OnCloseButtonClicked()
    {
        MenuChangeRequested?.Invoke(FullscreenMenuType.MainMenu);
    }

    public override void CloseMenu(float fade = 0)
    {
        CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
        base.CloseMenu(fade);
    }

    public override void OpenMenu(float fade = 0)
    {
        CloseButton.onClick.AddListener(OnCloseButtonClicked);
        if (WeaponCells == null)
        {
            WeaponCells = new List<WeaponShopCell>();
        }
        foreach (var weaponCell in WeaponCells)
        {
            var weaponSpec = Options.GetPlayerWeaponSpec(weaponCell.WeaponDef);
            if (weaponSpec == null)
            {
                weaponCell.gameObject.SetActive(false);
            }
            else
            {
                weaponCell.Initialize(weaponSpec);
            }
        }
        base.OpenMenu(fade);
    }

    public void OnCostChangedReceived()
    {
        var cost = 0;
        foreach (var cell in WeaponCells)
        {
            cost += cell.GetCost();
        }
        TotalCostText.text = cost.ToString();
        Options.CurrentWeaponCost = cost;
    }
}