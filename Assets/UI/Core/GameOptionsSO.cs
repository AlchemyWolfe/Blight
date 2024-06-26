using System;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "GameData/GameOptions", fileName = "SO_GameOptions")]
public class GameOptionsSO : ScriptableObject
{
    [Serializable]
    public class PlayerWeaponSetup
    {
        [SerializeField]
        public WeaponPoolSO Weapon;
        [SerializeField]
        public int WeaponLevel;
        [SerializeField]
        private int _minWeaponLevel;
        public int MinWeaponLevel
        {
            get => _minWeaponLevel;
            set => _minWeaponLevel = value;
        }
        [SerializeField]
        private int _maxPurchasedWeaponLevel;
        public int MaxPurchasedWeaponLevel
        {
            get => _maxPurchasedWeaponLevel;
            set => _maxPurchasedWeaponLevel = value;
        }
        [SerializeField]
        public int ProjectileLevel;
        [SerializeField]
        private int _maxPurchasedProjectileLevel;
        public int MaxPurchasedProjectileLevel
        {
            get => _maxPurchasedProjectileLevel;
            set => _maxPurchasedProjectileLevel = value;
        }
        [SerializeField]
        public int WeaponCost;
        [SerializeField]
        public int ProjectileCost;
    }

    [SerializeField]
    public List<PlayerWeaponSetup> PlayerWeaponSpecs;
    public int CurrentWeaponCost;

    public PlayerWeaponSetup GetPlayerWeaponSpec(WeaponPoolSO weaponDef)
    {
        foreach (var weaponSpec in PlayerWeaponSpecs)
        {
            if (weaponSpec.Weapon == weaponDef)
            {
                return weaponSpec;
            }
        }
        return null;
    }

    public void ResetPlayerWeaponCost()
    {
        var first = PlayerWeaponSpecs[0];
        foreach (var weaponSpec in PlayerWeaponSpecs)
        {
            weaponSpec.WeaponLevel = weaponSpec.MinWeaponLevel;
            weaponSpec.ProjectileLevel = 1;
        }
        CurrentWeaponCost = 0;
    }

    [SerializeField]
    private bool _mute;
    public bool Mute
    {
        get => _mute;
        set
        {
            _mute = value;
            Save();
        }
    }

    [SerializeField]
    private float _volume;
    public float Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            Save();
        }
    }

    public void Save()
    {
        ES3.Save<bool>("Mute", Mute, ES3Settings.defaultSettings.path);
        ES3.Save<float>("Volume", Volume, ES3Settings.defaultSettings.path);
    }

    public void Load()
    {
        Mute = ES3.Load<bool>("Mute", ES3Settings.defaultSettings.path);
        Volume = ES3.Load<float>("Volume", ES3Settings.defaultSettings.path);
    }
}
