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

        // Determine viable weapons
        foreach (var weapon in Tools.Player.Weapons)
        {
            if (weapon.WeaponLevel > 0)
            {
                viableWeapons.Add(weapon);
            }
        }

        //30% chance to upgrate one trait of an existing weapon.
        if (Random.value < 0.3f)
        {
            var idx = Random.Range(0, viableWeapons.Count);
            var weapon = viableWeapons[idx];
            LeftButton.InitializeWeapon(weapon, UpgradeButton.UpgradeType.Weapon, color);
            RightButton.InitializeWeapon(weapon, UpgradeButton.UpgradeType.Projectile, color);
            return;
        }

        var newWeapons = new List<Weapon>();
        foreach (var weapon in Tools.Player.Weapons)
        {
            if (weapon.WeaponLevel == 0)
            {
                newWeapons.Add(weapon);
            }
        }

        // 75% chance to upgrade projectiles or weapon levels of 2 viable weapons.
        if (viableWeapons.Count >= 2 || newWeapons.Count == 0 || Random.value < 0.75f)
        {
            // Leaning towards projectile upgrade
            var upgradeType = Random.value < 0.75f ? UpgradeButton.UpgradeType.Projectile : UpgradeButton.UpgradeType.Weapon;
            var idx1 = Random.Range(0, viableWeapons.Count);
            var weapon1 = viableWeapons[idx1];
            viableWeapons.RemoveAt(idx1);
            if (viableWeapons.Count == 0)
            {
                // We only hade one viable weapon and no new weapons?  Fall back to option 1.
                LeftButton.InitializeWeapon(weapon1, UpgradeButton.UpgradeType.Weapon, color);
                RightButton.InitializeWeapon(weapon1, UpgradeButton.UpgradeType.Projectile, color);
            }
            else
            {
                var idx2 = Random.Range(0, viableWeapons.Count);
                var weapon2 = viableWeapons[idx2];
                LeftButton.InitializeWeapon(weapon1, upgradeType, color);
                RightButton.InitializeWeapon(weapon2, upgradeType, color);
            }
            return;
        }

        // Upgrade weapon level of a new weapon or an existing one.
        var idxNew = Random.Range(0, newWeapons.Count);
        var newWweapon = newWeapons[idxNew];
        var idxOld = Random.Range(0, viableWeapons.Count);
        var oldweapon = viableWeapons[idxOld];
        LeftButton.InitializeWeapon(newWweapon, UpgradeButton.UpgradeType.Weapon, color);
        RightButton.InitializeWeapon(oldweapon, UpgradeButton.UpgradeType.Weapon, color);
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