using TMPro;
using UnityEngine;

public class UpgradeButton : ButtonTextColor
{
    public enum UpgradeType
    {
        Weapon,
        Projectile
    }

    public TMP_Text NameText;
    public TMP_Text LevelTextL;
    public TMP_Text LevelTextR;
    public TMP_Text UpgradeText;

    public Sprite WeaponUpgradeIcon;
    public Sprite ProjectileUpgradeIcon;

    public Weapon ActiveWeapon;
    public UpgradeType Type;

    private static string WeaponDisplayText = "Weapon";
    private static string ProjectileDisplayText = "Damage";

    // Start is called before the first frame update
    void Start()
    {
        AddTextGraphic(NameText);
        AddTextGraphic(LevelTextL);
        AddTextGraphic(LevelTextR);
        AddTextGraphic(UpgradeText);
    }

    public void InitializeWeapon(Weapon weapon, UpgradeType type, Color color)
    {
        ActiveWeapon = weapon;
        Type = type;
        NameText.text = weapon.DisplayName;
        if (type == UpgradeButton.UpgradeType.Weapon)
        {
            UpgradeText.text = WeaponDisplayText;
            var toLevel = weapon.WeaponLevel + 1;
            LevelTextL.text = weapon.WeaponLevel.ToString();
            LevelTextR.text = toLevel.ToString();
            TextButton.image.sprite = WeaponUpgradeIcon;
        }
        else
        {
            UpgradeText.text = ProjectileDisplayText;
            var toLevel = weapon.ProjectileLevel + 1;
            LevelTextL.text = weapon.ProjectileLevel.ToString();
            LevelTextR.text = toLevel.ToString();
            TextButton.image.sprite = ProjectileUpgradeIcon;
        }
        TextButton.image.color = color;
    }
}
