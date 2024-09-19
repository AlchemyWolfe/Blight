using MalbersAnimations.Controller;
using System;
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

    public SkinMaterialsSO SkinColors;
    public MagicMaterialsSO MagicColors;
    public SkinnedMeshRenderer Skin;
    public SkinnedMeshRenderer Magic;
    public SkinnedMeshRenderer Secondary;
    public GameObject ProjectileContainer;
    public GameSceneToolsSO Tools;

    [HideInInspector]
    public bool isAttacking;
    //[HideInInspector]
    private bool _isDying;
    public bool IsDying
    {
        get => _isDying;
        set
        {
            _isDying = value;
        }
    }
    public void LegitResurrection()
    {
        _isDying = false;
    }
    [HideInInspector]
    public int MagicColor;
    [HideInInspector]
    public Material MagicMaterial;
    [HideInInspector]
    public WorldHealthBar HealthBar;
    [HideInInspector]
    public Action<BlightCreature> OnKilled;

    protected void InitializeWeapons()
    {
        if (ProjectileContainer == null)
        {
            ProjectileContainer = Wielder.gameObject.transform.parent.gameObject;
        }
        foreach (var weapon in Weapons)
        {
            weapon.Wielder = Wielder;
            weapon.Muzzle = Muzzle;
            weapon.ProjectileContainer = ProjectileContainer;
        }
    }

    public void AddWeapon(Weapon weapon)
    {
        Weapons ??= new List<Weapon>();
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
    }

    public void SetSkinColor(int skinChoice)
    {
        if (skinChoice < 0 || SkinColors == null || skinChoice >= SkinColors.SkinMaterials.Count)
        {
            return;
        }
        skinChoice = skinChoice % SkinColors.SkinMaterials.Count;
        SetSkinColor(SkinColors.SkinMaterials[skinChoice]);
    }

    public void SetSkinColor(Material material)
    {
        if (Skin == null)
        {
            return;
        }
        Skin.material = material;
    }

    public void SetMagicColor(int magicChoice)
    {
        MagicColor = magicChoice;
        if (Magic == null)
        {
            return;
        }
        Magic.gameObject.SetActive(magicChoice >= 0);
        if (magicChoice < 0 || MagicColors == null)
        {
            return;
        }
        magicChoice = magicChoice % MagicColors.MagicMaterials.Count;
        MagicMaterial = MagicColors.MagicMaterials[magicChoice];
        SetMagicColor(MagicMaterial);
    }

    public void SetMagicColor(Material material)
    {
        if (Magic == null)
        {
            return;
        }
        Magic.material = material;
    }

    public void SetPositionOnGround()
    {
        SetPositionOnGround(transform.position);
    }

    public void SetPositionOnGround(Vector3 position)
    {
        if (Tools == null)
        {
            return;
        }
        // First, force position to be on the terrain
        Vector3 terrainPosition = Tools.Ter.transform.position;
        Vector3 terrainSize = Tools.Ter.terrainData.size;

        position.x = Mathf.Clamp(position.x, terrainPosition.x, terrainPosition.x + terrainSize.x);
        position.z = Mathf.Clamp(position.z, terrainPosition.z, terrainPosition.z + terrainSize.z);

        // Then make sure y matches the height of the terrain.
        position.y = Tools.Ter.SampleHeight(position);
        gameObject.transform.position = position;
    }

    public void SetPositionOnGround(Vector2 position)
    {
        SetPositionOnGround(new Vector3(position.x, Tools.Player.transform.position.y, position.y));
    }
}
