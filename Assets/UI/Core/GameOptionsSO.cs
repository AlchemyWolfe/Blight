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
    public String Version;

    [SerializeField]
    public String VersionDate;

    [SerializeField]
    public List<PlayerWeaponSetup> PlayerWeaponSpecs;
    public int CurrentWeaponCost;

    public System.Action OnMusicCheckmarkChanged;

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

    [SerializeField]
    private bool _enableMusic;
    public bool EnableMusic
    {
        get => _enableMusic;
        set
        {
            var changed = _enableMusic != value;
            _enableMusic = value;
            if (changed)
            {
                OnMusicCheckmarkChanged?.Invoke();
            }
            Save();
        }
    }

    [SerializeField]
    private bool _enablePant;
    public bool EnablePant
    {
        get => _enablePant;
        set
        {
            _enablePant = value;
            Save();
        }
    }

    [SerializeField]
    private bool _showPowerUpIndicators;
    public bool ShowPowerupIndicators
    {
        get => _showPowerUpIndicators;
        set
        {
            _showPowerUpIndicators = value;
            Save();
        }
    }

    [SerializeField]
    private bool _showShieldIndicators;
    public bool ShowShieldIndicators
    {
        get => _showShieldIndicators;
        set
        {
            _showShieldIndicators = value;
            Save();
        }
    }

    [SerializeField]
    private bool _showGemIndicators;
    public bool ShowGemIndicators
    {
        get => _showGemIndicators;
        set
        {
            _showGemIndicators = value;
            Save();
        }
    }

    public void Save()
    {
        ES3.Save<bool>("Mute", Mute, ES3Settings.defaultSettings.path);
        ES3.Save<float>("Volume", Volume, ES3Settings.defaultSettings.path);
        ES3.Save<bool>("Music", EnableMusic, ES3Settings.defaultSettings.path);
        ES3.Save<bool>("Pant", EnablePant, ES3Settings.defaultSettings.path);
        ES3.Save<bool>("ShowPowerupIndicators", ShowPowerupIndicators, ES3Settings.defaultSettings.path);
        ES3.Save<bool>("ShowShieldIndicators", ShowShieldIndicators, ES3Settings.defaultSettings.path);
        ES3.Save<bool>("ShowGemIndicators", ShowGemIndicators, ES3Settings.defaultSettings.path);
    }

    public void Load()
    {
        Mute = ES3.Load<bool>("Mute", ES3Settings.defaultSettings.path);
        Volume = ES3.Load<float>("Volume", ES3Settings.defaultSettings.path);
        EnableMusic = ES3.Load<bool>("Music", ES3Settings.defaultSettings.path);
        EnablePant = ES3.Load<bool>("Pant", ES3Settings.defaultSettings.path);
        ShowPowerupIndicators = ES3.Load<bool>("ShowPowerupIndicators", ES3Settings.defaultSettings.path);
        ShowShieldIndicators = ES3.Load<bool>("ShowShieldIndicators", ES3Settings.defaultSettings.path);
        ShowGemIndicators = ES3.Load<bool>("ShowGemIndicators", ES3Settings.defaultSettings.path);
    }
}
