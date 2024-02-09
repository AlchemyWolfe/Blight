using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "GameData/PlayerData", fileName = "SO_PlayerData")]
public class PlayerDataSO : ScriptableObject
{
    [SerializeField]
    private float _gameWave;
    public float GameWave
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
    //[HideInInspector]
    public float EarnedShield;
    //[HideInInspector]
    public float EarnedGems;
}
