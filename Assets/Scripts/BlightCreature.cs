using MalbersAnimations.Controller;
using System.Collections.Generic;
using UnityEngine;

public class BlightCreature : MonoBehaviour
{
    [Tooltip("The Animal component.")]
    public MAnimal Wielder;

    [Tooltip("The point to fire from, plus the projectile's initial orientation.")]
    public GameObject Muzzle;

    [Tooltip("Initial weapons this creature always starts with.  Probably only the player.")]
    public List<Weapon> Weapons;

    public MagicShield Shield;

    [Tooltip("This should be set to Internal Components, CameraTarget.")]
    public GameObject Center;

    public SkinnedMeshRenderer Skin;
    public SkinnedMeshRenderer Magic;
    public SkinnedMeshRenderer Secondary;

    private GameObject ProjectileContainer;

    protected void InitializeWeapons()
    {
        ProjectileContainer = Wielder.gameObject.transform.parent.gameObject;
        foreach (var weapon in Weapons)
        {
            weapon.Wielder = Wielder;
            weapon.Muzzle = Muzzle;
            weapon.ProjectileContainer = ProjectileContainer;
        }
    }

    public void AddWeapon(WeaponPoolSO weaponPool, int weaponLevel, int projectileLevel)
    {
        if (Weapons == null)
        {
            Weapons = new List<Weapon>();
        }
        var weapon = weaponPool.CreateWeapon(Wielder, Muzzle, ProjectileContainer, weaponLevel, projectileLevel);
        weapon.StartAttacking();
        Weapons.Add(weapon);
    }

    public void StartAttacking()
    {
        if (Weapons == null)
        {
            return;
        }
        foreach (var weapon in Weapons)
        {
            weapon.StartAttacking();
        }
    }

    public void StopAttacking()
    {
        if (Weapons == null)
        {
            return;
        }
        foreach (var weapon in Weapons)
        {
            weapon.StopAttacking();
        }
        Weapons.Clear();
    }

    public void SetSkin(Material material)
    {
        if (Skin == null)
        {
            Skin = GetComponent<SkinnedMeshRenderer>();
        }
        if (Skin != null)
        {
            Skin.material = material;
        }
    }

    public void SetMagic(bool isMagic)
    {
        Magic.enabled = isMagic;
    }

    public void GetMagicMaterial()
    {

    }
}
