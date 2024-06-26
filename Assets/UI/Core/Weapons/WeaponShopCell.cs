using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponShopCell : MonoBehaviour
{
    public WeaponPoolSO WeaponDef;
    public TMP_Text WeaponName;
    public TMP_Text WeaponLevelCostText;
    public Button WeaponUpButton;
    public Button WeaponDownButton;
    public TMP_Text WeaponLevelText;
    public GameObject ProjctileContainer;
    public TMP_Text ProjectileLevelCostText;
    public Button ProjctileUpButton;
    public Button ProjctileDownButton;
    public TMP_Text ProjctileLevelText;

    private int InitialWeaponLevel;
    private int InitialProjectileLevel;
    private int WeaponCost;
    private int ProjectileCost;

    public Action OnCostChanged;
    private GameOptionsSO.PlayerWeaponSetup WeaponSpec;

    private void Awake()
    {
        WeaponUpButton.onClick.AddListener(OnWeaponUpButtonClicked);
        WeaponDownButton.onClick.AddListener(OnWeaponDownButtonClicked);
        ProjctileUpButton.onClick.AddListener(OnProjctileUpButtonClicked);
        ProjctileDownButton.onClick.AddListener(OnProjctileDownButtonClicked);
    }

    public void Initialize(GameOptionsSO.PlayerWeaponSetup weaponSpec)
    {
        WeaponSpec = weaponSpec;
        WeaponName.text = weaponSpec.Weapon.WeaponPrefab.DisplayName;
        InitialWeaponLevel = WeaponSpec.WeaponLevel = Mathf.Max(0, weaponSpec.WeaponLevel);
        WeaponLevelText.text = WeaponSpec.WeaponLevel.ToString();
        InitialProjectileLevel = WeaponSpec.ProjectileLevel = Mathf.Max(1, weaponSpec.ProjectileLevel);
        ProjctileLevelText.text = WeaponSpec.ProjectileLevel.ToString();
        WeaponCost = weaponSpec.WeaponCost;
        WeaponLevelCostText.text = WeaponCost.ToString();
        ProjectileCost = weaponSpec.ProjectileCost;
        ProjectileLevelCostText.text = ProjectileCost.ToString();
        UpdateEnabledStates();
    }

    private void UpdateEnabledStates()
    {
        var projectileEnabled = WeaponSpec.WeaponLevel > 0;
        var pupEnabled = projectileEnabled && (WeaponSpec.ProjectileLevel < WeaponSpec.MaxPurchasedProjectileLevel);
        var pdwnEnabled = projectileEnabled && (WeaponSpec.ProjectileLevel > 1);
        var wupEnabled = WeaponSpec.WeaponLevel < WeaponSpec.MaxPurchasedWeaponLevel;
        var wdwnEnabled = WeaponSpec.WeaponLevel > WeaponSpec.MinWeaponLevel;
        ProjctileUpButton.enabled = pupEnabled;
        ProjctileDownButton.enabled = pdwnEnabled;
        WeaponUpButton.enabled = wupEnabled;
        WeaponDownButton.enabled = wdwnEnabled;
    }

    public int GetCost()
    {
        var weaponLevelDelta = WeaponSpec.WeaponLevel - InitialWeaponLevel;
        var projectileLevelDelta = WeaponSpec.ProjectileLevel - InitialProjectileLevel;
        return (weaponLevelDelta * WeaponCost) + (projectileLevelDelta * ProjectileCost);
    }

    public void OnWeaponUpButtonClicked()
    {
        Debug.Log(WeaponName.text + " Weapon Up Clicked");
        WeaponSpec.WeaponLevel++;
        WeaponLevelText.text = WeaponSpec.WeaponLevel.ToString();
        WeaponDownButton.enabled = true;
        OnCostChanged?.Invoke();
        UpdateEnabledStates();
    }

    public void OnWeaponDownButtonClicked()
    {
        Debug.Log(WeaponName.text + " Weapon Down Clicked");
        WeaponSpec.WeaponLevel--;
        WeaponLevelText.text = WeaponSpec.WeaponLevel.ToString();
        if (WeaponSpec.WeaponLevel <= WeaponSpec.MinWeaponLevel)
        {
            WeaponDownButton.enabled = false;
        }
        OnCostChanged?.Invoke();
        UpdateEnabledStates();
    }

    public void OnProjctileUpButtonClicked()
    {
        Debug.Log(WeaponName.text + " Projctile Up Clicked");
        WeaponSpec.ProjectileLevel++;
        ProjctileLevelText.text = WeaponSpec.ProjectileLevel.ToString();
        OnCostChanged?.Invoke();
        UpdateEnabledStates();
    }

    public void OnProjctileDownButtonClicked()
    {
        Debug.Log(WeaponName.text + " Projctile Down Clicked");
        WeaponSpec.ProjectileLevel--;
        ProjctileLevelText.text = WeaponSpec.ProjectileLevel.ToString();
        OnCostChanged?.Invoke();
        UpdateEnabledStates();
    }
}
