using System;
using UnityEngine;

//[CreateAssetMenu(menuName = "GameData/PlayerData", fileName = "SO_PlayerData")]
public class PlayerDataSO : ScriptableObject
{
    [SerializeField]
    private int _gameWave;
    public int GameWave
    {
        get
        {
            return _gameWave;
        }
        set
        {
            _gameWave = value;
            OnGameWaveChanged?.Invoke();
        }
    }

    [SerializeField]
    private float _gameScore;
    public float GameScore
    {
        get
        {
            return _gameScore;
        }
        set
        {
            _gameScore = value;
            OnGameScoreChanged?.Invoke();
        }
    }

    [SerializeField]
    private float _highScore;
    public float HighScore
    {
        get
        {
            return _highScore;
        }
        set
        {
            _highScore = value;
            OnHighScoreChanged?.Invoke();
        }
    }

    [SerializeField]
    private float _gameGems;
    public float GameGems
    {
        get
        {
            return _gameGems;
        }
        set
        {
            _gameGems = value;
            OnGameGemsChanged?.Invoke();
        }
    }

    [SerializeField]
    private float _totalGems;
    public float TotalGems
    {
        get
        {
            return _totalGems;
        }
        set
        {
            _totalGems = value;
            OnTotalGemsChanged?.Invoke();
        }
    }

    [SerializeField]
    private int _highestWave;
    public int HighestWave
    {
        get
        {
            return _highestWave;
        }
        set
        {
            _highestWave = value;
        }
    }

    [SerializeField]
    private int _chosenSkin;
    public int ChosenSkin
    {
        get
        {
            return _chosenSkin;
        }
        set
        {
            _chosenSkin = value;
            OnSkinChoiceChanged?.Invoke();
            ES3.Save<int>("ChosenSkin", ChosenSkin, ES3Settings.defaultSettings.path);
        }
    }

    [SerializeField]
    private int _chosenMagic;
    public int ChosenMagic
    {
        get
        {
            return _chosenMagic;
        }
        set
        {
            _chosenMagic = value;
            OnMagicChoiceChanged?.Invoke();
            ES3.Save<int>("ChosenMagic", ChosenMagic, ES3Settings.defaultSettings.path);
        }
    }

    [HideInInspector]
    private int _shieldNeed;
    public int ShieldNeed
    {
        get
        {
            return _shieldNeed;
        }
        set
        {
            _shieldNeed = value;
            OnShieldNeedChanged?.Invoke();
        }
    }

    [HideInInspector]
    public Action OnGameGemsChanged;
    [HideInInspector]
    public Action OnGameScoreChanged;
    [HideInInspector]
    public Action OnShieldNeedChanged;
    [HideInInspector]
    public Action OnTotalGemsChanged;
    [HideInInspector]
    public Action OnHighScoreChanged;
    [HideInInspector]
    public Action OnGameWaveChanged;
    [HideInInspector]
    public Action OnSkinChoiceChanged;
    [HideInInspector]
    public Action OnMagicChoiceChanged;

    // Not recorded.
    public float EarnedShield;
    public float EarnedGems;
    public float MissedUpgrades;
    public float PreviousGems;
    public int PreviousHighestWave;
    public float PreviousHighScore;

    public void Save()
    {
        ES3.Save<float>("HighScore", HighScore, ES3Settings.defaultSettings.path);
        ES3.Save<float>("TotalGems", TotalGems, ES3Settings.defaultSettings.path);
        ES3.Save<int>("HighestWave", HighestWave, ES3Settings.defaultSettings.path);
        ES3.Save<int>("ChosenSkin", ChosenSkin, ES3Settings.defaultSettings.path);
        ES3.Save<int>("ChosenMagic", ChosenMagic, ES3Settings.defaultSettings.path);
    }

    public void Load()
    {
        HighScore = ES3.Load<float>("HighScore", ES3Settings.defaultSettings.path);
        TotalGems = ES3.Load<float>("TotalGems", ES3Settings.defaultSettings.path);
        HighestWave = ES3.Load<int>("HighestWave", ES3Settings.defaultSettings.path);
        ChosenSkin = ES3.Load<int>("ChosenSkin", ES3Settings.defaultSettings.path);
        ChosenMagic = ES3.Load<int>("ChosenMagic", ES3Settings.defaultSettings.path);
    }
}
