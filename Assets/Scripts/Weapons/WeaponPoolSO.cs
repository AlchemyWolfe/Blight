using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(menuName = "Blight/WeaponPool", fileName = "SO_WeaponPool_")]
public class WeaponPoolSO : ScriptableObject
{
    [SerializeField]
    public Weapon WeaponPrefab;

    [SerializeField, HideInInspector]
    public ObjectPool<Weapon> WeaponPool;

    public bool DebugLogs;

    public void Initialize()
    {
        if (WeaponPool == null)
        {
            WeaponPool = new ObjectPool<Weapon>(OnCreateWeapon, OnGetWeapon, OnReleaseWeapon, OnDestroyWeapon, false, 10, 100);
        }
        WeaponPool.Clear();
    }

    public Weapon CreateWeapon(BlightCreature creature, int weaponLevel, int projectileLevel)
    {
        var Weapon = WeaponPool.Get();
        Weapon.SetLevelValues(weaponLevel, projectileLevel);
        Weapon.Equip(creature);
        Weapon.Initialize();
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
