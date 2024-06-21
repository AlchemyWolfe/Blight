using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeMenuController : FullScreenMenuController
{
    public GameSceneToolsSO Tools;
    public MagicMaterialsSO MagicColors;

    public UpgradeButton LeftButton;
    public UpgradeButton RightButton;

    public override FullscreenMenuType Type { get => FullscreenMenuType.Upgrade; }

    public void ChooseUpgrades()
    {
        var color = MagicColors.MagicColors[Tools.Player.MagicColor];
        var viableWeapons = new List<Weapon>();
        // Upgrade an existing weapon
        foreach (var weapon in Tools.Player.Weapons)
        {
            if (weapon.WeaponLevel > 0)
            {
                viableWeapons.Add(weapon);
            }
        }
        if (Random.value < 0.6f)
        {
            var idx = Random.Range(0, viableWeapons.Count);
            var weapon = viableWeapons[idx];
            LeftButton.InitializeWeapon(weapon, UpgradeButton.UpgradeType.Weapon, color);
            RightButton.InitializeWeapon(weapon, UpgradeButton.UpgradeType.Projectile, color);
        }
        else
        {
            foreach (var weapon in Tools.Player.Weapons)
            {
                viableWeapons.Add(weapon);
            }
            var idx = Random.Range(0, viableWeapons.Count);
            var weapon1 = viableWeapons[idx];
            viableWeapons.RemoveAt(idx);
            idx = Random.Range(0, viableWeapons.Count);
            var weapon2 = viableWeapons[idx];
            var type = (weapon1.WeaponLevel == 0 || weapon2.WeaponLevel == 0) ? UpgradeButton.UpgradeType.Weapon : (Random.value < 0.5f) ? UpgradeButton.UpgradeType.Weapon : UpgradeButton.UpgradeType.Projectile;
            LeftButton.InitializeWeapon(weapon1, type, color);
            RightButton.InitializeWeapon(weapon2, type, color);
        }
    }

    public override void EnableControls(bool enabled)
    {
        LeftButton.enabled = enabled;
        RightButton.enabled = enabled;
    }

    private void UpgradeWeapon(UpgradeButton upgradeButton)
    {
        if (upgradeButton.Type == UpgradeButton.UpgradeType.Weapon)
        {
            upgradeButton.ActiveWeapon.UpgradeWeapon();
        }
        else
        {
            upgradeButton.ActiveWeapon.UpgradeProjectile();
        }
    }

    private void OnLeftChoiceButtonClicked()
    {
        UpgradeWeapon(LeftButton);
        MenuChangeRequested?.Invoke(FullscreenMenuType.Game);
    }

    private void OnRightChoiceButtonClicked()
    {
        UpgradeWeapon(RightButton);
        MenuChangeRequested?.Invoke(FullscreenMenuType.Game);
    }

    public override void CloseMenu(float fade = 0)
    {
        Time.timeScale = 1f;
        DOTween.PlayAll();
        LeftButton.TextButton.onClick.RemoveListener(OnLeftChoiceButtonClicked);
        RightButton.TextButton.onClick.RemoveListener(OnRightChoiceButtonClicked);
        base.CloseMenu(fade);
    }

    public override void OpenMenu(float fade = 0)
    {
        Time.timeScale = 0f;
        DOTween.PauseAll();
        LeftButton.TextButton.onClick.AddListener(OnLeftChoiceButtonClicked);
        RightButton.TextButton.onClick.AddListener(OnRightChoiceButtonClicked);
        ChooseUpgrades();
        base.OpenMenu(fade);
    }
}