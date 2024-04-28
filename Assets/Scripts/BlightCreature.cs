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
    public GameObject ProjectileContainer;

    private bool isAttacking;

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

    public void AddWeapon(Weapon weapon)
    {
        if (Weapons == null)
        {
            Weapons = new List<Weapon>();
        }
        if (isAttacking)
        {
            weapon.StartAttacking();
        }
        Weapons.Add(weapon);
    }

    public void StartAttacking()
    {
        isAttacking = true;
        if (Weapons == null)
        {
            return;
        }
        foreach (var weapon in Weapons)
        {
            if (!weapon.IsFiring)
            {
                weapon.StartAttacking();
            }
        }
    }

    public void StopAttacking()
    {
        isAttacking = false;
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
        if (Magic != null)
        {
            Magic.enabled = isMagic;
        }
    }

    public void GetMagicMaterial()
    {

    }
}
