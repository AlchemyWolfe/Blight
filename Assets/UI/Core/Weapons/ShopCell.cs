using TMPro;
using UnityEngine;

public class ShopCell : MonoBehaviour
{
    public WeaponPoolSO WeaponDef;
    public TMP_Text ItemName;
    public ShopCellDetail WeaponLevel;
    public ShopCellDetail ProjectileLevel;

    public int TotalCost
    {
        get
        {
            return WeaponLevel.TotalCost + ProjectileLevel.TotalCost;
        }
    }

    [HideInInspector]
    public System.Action OnCostChanged;

    private GameOptionsSO.PlayerWeaponSetup WeaponSpec;

    void Start()
    {
    }

    public void Initialize(GameOptionsSO.PlayerWeaponSetup weaponSpec)
    {
        WeaponSpec = weaponSpec;
        ItemName.text = weaponSpec.Weapon.WeaponPrefab.DisplayName;
        WeaponLevel.Initialize(weaponSpec.WeaponCost, weaponSpec.WeaponLevel, weaponSpec.MinWeaponLevel, weaponSpec.MaxPurchasedWeaponLevel);
        ProjectileLevel.Initialize(weaponSpec.ProjectileCost, weaponSpec.ProjectileLevel, 1, weaponSpec.MaxPurchasedProjectileLevel, WeaponLevel.DetailLevel > 0);
        WeaponLevel.OnLevelChanged += OnLevelChangeRecieved;
        ProjectileLevel.OnLevelChanged += OnLevelChangeRecieved;
    }

    public void OnLevelChangeRecieved()
    {
        WeaponLevel.UpdateButtons();
        if (WeaponLevel.DetailLevel == 0)
        {
            ProjectileLevel.Reset();
            ProjectileLevel.SetEnabled(false);
        }
        else
        {
            ProjectileLevel.SetEnabled(true);
        }
        OnCostChanged?.Invoke();
    }

    public void ApplyPurchase()
    {
        WeaponSpec.WeaponLevel = WeaponLevel.DetailLevel;
        WeaponSpec.ProjectileLevel = ProjectileLevel.DetailLevel;
    }
}
