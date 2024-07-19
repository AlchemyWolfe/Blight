using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponsMenuController : FullScreenMenuController
{
    public GameOptionsSO Options;
    [SerializeField]
    private PlayerDataSO _playerData;
    public PlayerDataSO PlayerData { get => _playerData; set => _playerData = value; }

    [SerializeField]
    private Button _closeButton;
    public Button CloseButton => _closeButton;

    public GameObject ShopMenu;
    public TMP_Text GemsRemainingText;
    public List<ShopCell> ShopCells;

    [SerializeField]
    private Button _selectButton;
    public Button SelectButton => _selectButton;
    private ButtonTextColor SelectTextColor;

    public override FullscreenMenuType Type { get => FullscreenMenuType.Weapons; }

    private void Awake()
    {
        foreach (var cell in ShopCells)
        {
            cell.OnCostChanged += OnCostChangedReceived;
        }
        SelectTextColor = SelectButton.GetComponent<ButtonTextColor>();
    }
    public override void EnableControls(bool enabled)
    {
        CloseButton.enabled = enabled;
        SelectButton.enabled = enabled;
    }

    private void OnCloseButtonClicked()
    {
        MenuChangeRequested?.Invoke(FullscreenMenuType.MainMenu);
    }

    public override void CloseMenu(float fade = 0)
    {
        CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
        SelectButton.onClick.RemoveListener(OnSelectButtonClicked);
        base.CloseMenu(fade);
    }

    public override void OpenMenu(float fade = 0)
    {
        CloseButton.onClick.AddListener(OnCloseButtonClicked);
        SelectButton.onClick.AddListener(OnSelectButtonClicked);
        if (ShopCells == null)
        {
            ShopCells = new List<ShopCell>();
        }
        foreach (var cell in ShopCells)
        {
            var weaponSpec = Options.GetPlayerWeaponSpec(cell.WeaponDef);
            if (weaponSpec == null)
            {
                cell.gameObject.SetActive(false);
            }
            else
            {
                cell.Initialize(weaponSpec);
            }
        }
        OnCostChangedReceived();
        base.OpenMenu(fade);
    }

    public void OnCostChangedReceived()
    {
        var cost = 0;
        foreach (var cell in ShopCells)
        {
            cost += cell.TotalCost;
        }
        var remaining = PlayerData.TotalGems - cost;
        GemsRemainingText.text = remaining.ToString();
        SelectButton.enabled = remaining >= 0;
        SelectTextColor.UpdateEnabledState();
    }

    private void OnSelectButtonClicked()
    {
        var cost = 0;
        foreach (var cell in ShopCells)
        {
            cell.ApplyPurchase();
            cost += cell.TotalCost;
        }
        Options.CurrentWeaponCost = cost;
        MenuChangeRequested?.Invoke(FullscreenMenuType.MainMenu);
    }
}