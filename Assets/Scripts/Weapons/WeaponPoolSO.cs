using MalbersAnimations.Controller;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(menuName = "Blight/WeaponPool", fileName = "SO_WeaponPool_")]
public class WeaponPoolSO : ScriptableObject
{
    [SerializeField]
    public Weapon WeaponPrefab;

    [SerializeField, HideInInspector]
    public ObjectPool<Weapon> WeaponPool;

    public void Initialize()
    {
        if (WeaponPool == null)
        {
            WeaponPool = new ObjectPool<Weapon>(OnCreateWeapon, OnGetWeapon, OnReleaseWeapon, OnDestroyWeapon, false, 10, 100);
        }
        WeaponPool.Clear();
    }

    public Weapon CreateWeapon(MAnimal wielder, GameObject muzzle, GameObject projectileContainer, int weaponLevel, int projectileLevel)
    {
        var Weapon = WeaponPool.Get();
        Weapon.Wielder = wielder;
        Weapon.Muzzle = muzzle;
        Weapon.ProjectileContainer = projectileContainer;
        Weapon.WeaponLevel = weaponLevel;
        Weapon.ProjectileLevel = projectileLevel;
        Weapon.Initialize();
        Weapon.Equip(wielder, muzzle, projectileContainer);
        // Finally, be on the layer of my parent.
        Weapon.gameObject.layer = wielder.gameObject.layer;
        return Weapon;
    }

    public void ReturnWeapon(Weapon Weapon)
    {
        if (Weapon.InUse)
        {
            WeaponPool.Release(Weapon);
        }
    }

    private Weapon OnCreateWeapon()
    {
        var Weapon = GameObject.Instantiate(WeaponPrefab);
        Weapon.WeaponPool = this;
        return Weapon;
    }

    private void OnGetWeapon(Weapon Weapon)
    {
        Weapon.gameObject.SetActive(true);
        Weapon.InUse = true;
    }

    private void OnReleaseWeapon(Weapon Weapon)
    {
        Weapon.gameObject.SetActive(false);
        Weapon.InUse = false;
    }

    private void OnDestroyWeapon(Weapon Weapon)
    {
        Destroy(Weapon.gameObject);
    }
}
